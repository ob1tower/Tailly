using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ShopService.Application.Dtos.Requests.Product;
using Tailly.ShopService.Application.Dtos.Responses.Catalog;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Mappers;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Application.Validators;
using Tailly.ShopService.Core.Models.Catalog;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Infrastructure.Configurations.Extensions;

namespace Tailly.ShopService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<ReplyToReviewRequest> _replyValidator;
    private readonly IValidator<CreateProductReviewRequest> _createReviewValidator;

    public ProductController(IProductService productService, 
                             IValidator<ReplyToReviewRequest> replyValidator,
                             IValidator<CreateProductReviewRequest> createReviewValidator)
    {
        _productService = productService;
        _replyValidator = replyValidator;
        _createReviewValidator = createReviewValidator;
    }

    /// <summary>
    /// Gets a list of products.
    /// </summary>
    /// <param name="search">Search query by title or description.</param>
    /// <param name="categoryIds">List of category IDs to filter by (comma-separated in query).</param>
    /// <param name="minPrice">Minimum price filter.</param>
    /// <param name="maxPrice">Maximum price filter.</param>
    /// <param name="onlyAvailable">Show only available products (default: false).</param>
    /// <param name="sort">Sorting mode: popular, price-asc, price-desc, rating-desc, newest (default: newest).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 20, max: 100).</param>
    /// <returns>Paginated list of products and total count.</returns>
    [HttpGet("products")]
    [EnableRateLimiting("catalog")]
    public async Task<IActionResult> GetCatalog([FromQuery] string? search = null, [FromQuery] List<string>? categoryIds = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null, [FromQuery] bool onlyAvailable = false, [FromQuery] string? sort = "newest", [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var pagination = new PaginationValidator(page, limit);

        var filterModel = new CatalogFilterState
        {
            Search = search,
            CategoryIds = categoryIds ?? new List<string>(),
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            OnlyAvailable = onlyAvailable,
            Sort = ShopMapper.ParseProductSort(sort),
            Page = pagination.PageNumber,
            Limit = pagination.PageSize
        };

        var result = await _productService.GetCatalogAsync(filterModel);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (products, total) = result.Value;

        var response = new CatalogProductsResponse
        {
            Items = products.Select(p => p.ToCatalogResponse()).ToList(),
            Total = total,
            Page = pagination.PageNumber,
            Limit = pagination.PageSize
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets catalog metadata: all categories, minimum and maximum price, and available sorting options.
    /// </summary>
    /// <returns>Catalog metadata used for filters on the frontend.</returns>
    [HttpGet("catalog/meta")]
    [EnableRateLimiting("catalog")]
    public async Task<IActionResult> GetCatalogMeta()
    {
        var result = await _productService.GetCatalogMetaAsync();

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (categories, minPrice, maxPrice) = result.Value;

        var response = new CatalogMetaResponse
        {
            Categories = categories.Select(c => c.ToResponse()).ToList(),
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            AvailableSorts = new List<string> { "popular", "price-asc", "price-desc", "rating-desc", "newest" }
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets a single product by its SEO-friendly slug.
    /// </summary>
    /// <param name="slug">SEO-friendly URL slug of the product.</param>
    [HttpGet("products/{slug}")]
    [EnableRateLimiting("product")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _productService.GetBySlugAsync(slug);

        if (result.IsFailure)
            return NotFound(result.Error);

        var response = result.Value.ToResponse();        

        return Ok(response);
    }

    /// <summary>
    /// Gets multiple products by a comma-separated list of IDs.
    /// Used mainly for cart and order pages to load multiple products at once.
    /// </summary>
    /// <param name="ids">Comma-separated list of product GUIDs.</param>
    /// <returns>List of products.</returns>
    [HttpGet("products/by-ids")]
    [EnableRateLimiting("product-bulk")]
    public async Task<IActionResult> GetByIds([FromQuery] string ids)
    {
        var result = await _productService.GetByIdsAsync(ids);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var response = result.Value.Select(p => p.ToResponse()).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Creates a new product review.
    /// </summary>
    /// <param name="request">Review data including product, order, rating, and text.</param>
    /// <remarks>
    /// The user must be authenticated and have a completed order containing the product.
    /// Only one review per product per order is allowed.
    /// </remarks>
    /// <returns>Success status if the review was created.</returns>
    [Authorize(Roles = "Client, Specialist")]
    [HttpPost("reviews")]
    [EnableRateLimiting("reviews")]
    public async Task<IActionResult> CreateReview([FromBody] CreateProductReviewRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var validation = await _createReviewValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var review = new ProductReview
        {
            ProductId = request.ProductId,
            OrderId = request.OrderId,
            Rating = request.Rating,
            Text = request.Text
        };

        var result = await _productService.CreateReviewAsync(userId.Value, review);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Adds an admin reply to an existing review.
    /// </summary>
    /// <param name="reviewId">Review identifier.</param>
    /// <param name="request">Reply text.</param>
    /// <remarks>
    /// Only administrators can reply to reviews.  
    /// A review can have only one reply.
    /// </remarks>
    /// <returns>Success status if the reply was added.</returns>
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("reviews/{reviewId:guid}/reply")]
    public async Task<IActionResult> ReplyToReview(Guid reviewId, [FromBody] ReplyToReviewRequest request)
    {
        var validation = await _replyValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _productService.ReplyToReviewAsync(reviewId, request.Text);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }
}