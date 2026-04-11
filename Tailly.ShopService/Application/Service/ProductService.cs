using CSharpFunctionalExtensions;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Models.Catalog;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Application.Service;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<Product, Error>> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            _logger.LogWarning("GetBySlugAsync called with empty or whitespace slug");
            return Result.Failure<Product, Error>(ShopErrors.InvalidSlug);
        }

        var trimmedSlug = slug.Trim();

        var product = await _productRepository.GetBySlugAsync(trimmedSlug);

        if (product == null)
        {
            _logger.LogWarning("Product not found by slug: {Slug}", trimmedSlug);
            return Result.Failure<Product, Error>(ShopErrors.ProductNotFound);
        }

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

        filter.Page = Math.Max(1, filter.Page);
        filter.Limit = Math.Clamp(filter.Limit, 1, 100);

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
}