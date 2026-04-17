using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.Contracts.Messages;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Catalog;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Application.Service;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestClient<GetUserFullNameRequest> _client;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository,
                          IOrderRepository orderRepository,
                          IRequestClient<GetUserFullNameRequest> client,
                          ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _client = client;
        _logger = logger;
    }

    public async Task<Result<Product, Error>> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            _logger.LogWarning("GetBySlugAsync called with empty or whitespace slug.");
            return Result.Failure<Product, Error>(ShopErrors.InvalidSlug);
        }

        var trimmedSlug = slug.Trim();

        var product = await _productRepository.GetBySlugAsync(trimmedSlug);

        if (product == null)
        {
            _logger.LogWarning("Product not found by slug: {Slug}", trimmedSlug);
            return Result.Failure<Product, Error>(ShopErrors.ProductNotFound);
        }

        product.ReviewsCount = product.Reviews.Count;

        product.Rating = product.Reviews.Count == 0
            ? 0
            : Math.Round(product.Reviews.Average(r => (decimal)r.Rating), 1);

        _logger.LogInformation("Product retrieved successfully by slug: {Slug} - {Title}", trimmedSlug, product.Title);
        return Result.Success<Product, Error>(product);
    }

    public async Task<Result<List<Product>, Error>> GetByIdsAsync(string ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            _logger.LogWarning("GetByIdsAsync called with empty ids string");
            return Result.Failure<List<Product>, Error>(ShopErrors.InvalidProductIds);
        }

        var idList = ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (idList.Count == 0)
        {
            _logger.LogWarning("GetByIdsAsync called with invalid GUIDs: {Ids}", ids);
            return Result.Failure<List<Product>, Error>(ShopErrors.InvalidProductIds);
        }

        var products = await _productRepository.GetByIdsAsync(idList);

        _logger.LogInformation("Successfully retrieved {Count} products by IDs", products.Count);
        return Result.Success<List<Product>, Error>(products);
    }

    public async Task<Result<(List<Product> products, int total), Error>> GetCatalogAsync(CatalogFilterState filter)
    {
        filter ??= new CatalogFilterState();

        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice.Value > filter.MaxPrice.Value)
        {
            _logger.LogWarning("Invalid price range: Min={Min} > Max={Max}", filter.MinPrice, filter.MaxPrice);
            return Result.Failure<(List<Product> products, int total), Error>(ShopErrors.InvalidPriceRange);
        }

        var (products, total) = await _productRepository.GetCatalogAsync(
            filter.Search,
            filter.CategoryIds,
            filter.MinPrice,
            filter.MaxPrice,
            filter.OnlyAvailable,
            filter.Sort,
            filter.Page,
            filter.Limit);

        foreach (var product in products)
        {
            product.ReviewsCount = product.Reviews.Count;

            product.Rating = product.Reviews.Count == 0
                ? 0
                : Math.Round(product.Reviews.Average(r => (decimal)r.Rating), 1);
        }

        _logger.LogInformation("Catalog retrieved successfully. Page: {Page}, Items: {Count}, Total: {Total}",
            filter.Page, products.Count, total);

        return Result.Success<(List<Product> products, int total), Error>((products, total));
    }

    public async Task<Result<(List<Category> categories, decimal minPrice, decimal maxPrice), Error>> GetCatalogMetaAsync()
    {
        var result = await _productRepository.GetCatalogMetaAsync();

        _logger.LogInformation("Catalog meta retrieved: {CategoryCount} categories, price range {MinPrice} — {MaxPrice}",
            result.categories.Count, result.minPrice, result.maxPrice);

        return Result.Success<(List<Category> categories, decimal minPrice, decimal maxPrice), Error>(result);
    }

    public async Task<Result> ReplyToReviewAsync(Guid reviewId, string replyText)
    {
        var review = await _productRepository.GetReviewByIdAsync(reviewId);

        if (review == null)
            return Result.Failure(ShopErrors.ReviewNotFound.Description);

        if (review.Reply != null)
            return Result.Failure(ShopErrors.ReviewAlreadyHasReply.Description);

        var reply = new ProductReviewReply
        {
            Id = Guid.NewGuid(),
            AuthorName = "Tailly Shop",
            Text = replyText.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.AddReviewReplyAsync(reviewId, reply);

        _logger.LogInformation("Reply successfully added to review {ReviewId}", reviewId);

        return Result.Success();
    }

    public async Task<Result> CreateReviewAsync(Guid userId, ProductReview review)
    {
        var order = await _orderRepository.GetByIdAsync(review.OrderId);
        if (order == null || order.OwnerUserId != userId)
            return Result.Failure(ShopErrors.OrderNotFound.Description);

        if (order.Status != OrderStatus.Completed)
            return Result.Failure(ShopErrors.OrderNotCompleted.Description);

        if (!order.Items.Any(i => i.ProductId == review.ProductId))
            return Result.Failure(ShopErrors.ProductNotInOrder.Description);

        var alreadyReviewed = await _productRepository.HasReviewAsync(userId, review.OrderId, review.ProductId);
        if (alreadyReviewed)
            return Result.Failure(ShopErrors.ReviewAlreadyExists.Description);

        _logger.LogInformation("Requesting full name for user {UserId}", userId);

        var response = await _client.GetResponse<GetUserFullNameResponse>(
            new GetUserFullNameRequest
            {
                UserId = userId
            });

        var userFullName = $"{response.Message.FirstName} {response.Message.LastName}";

        _logger.LogInformation("Full name received: {FullName}", userFullName);

        review.Id = Guid.NewGuid();
        review.UserId = userId;
        review.AuthorName = userFullName;
        review.Text = review.Text.Trim();
        review.CreatedAt = DateTime.UtcNow;

        await _productRepository.AddReviewAsync(review);

        _logger.LogInformation("Review created for product {ProductId} by user {UserId}", review.ProductId, userId);

        return Result.Success();
    }
}