using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ShopService.Application.Dtos.Requests.Catalog;
using Tailly.ShopService.Application.Dtos.Responses.Catalog;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Mappers;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Models.Catalog;

namespace Tailly.ShopService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CatalogFilterRequest> _catalogFilterValidator;

    public ProductController(IProductService productService,
                             IValidator<CatalogFilterRequest> catalogFilterValidator)
    {
        _productService = productService;
        _catalogFilterValidator = catalogFilterValidator;
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
    /// Gets a paginated list of products with optional filtering and sorting.
    /// </summary>
    /// <param name="filter">Filter parameters: search, categories, price range, availability, sort, page, limit.</param>
    /// <returns>Paginated list of products and total count.</returns>
    [HttpGet("products")]
    [EnableRateLimiting("catalog")]
    public async Task<IActionResult> GetCatalog([FromQuery] CatalogFilterRequest filter)
    {
        var validationResult = await _catalogFilterValidator.ValidateAsync(filter);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var filterModel = new CatalogFilterState
        {
            Search = filter.Search,
            CategoryIds = filter.CategoryIds ?? new List<string>(),
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            OnlyAvailable = filter.OnlyAvailable,
            Sort = ShopMapper.ParseProductSort(filter.Sort),
            Page = filter.Page,
            Limit = filter.Limit
        };

        var result = await _productService.GetCatalogAsync(filterModel);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (products, total) = result.Value;

        var response = new CatalogProductsResponse
        {
            Items = products.Select(p => p.ToCatalogResponse()).ToList(),
            Total = total,
            Page = filter.Page,
            Limit = filter.Limit
        };

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
}