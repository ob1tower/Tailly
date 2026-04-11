using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Products;

namespace Tailly.ShopService.Infrastructure.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<(List<Product> products, int total)> GetCatalogAsync(string? search, List<string>? categoryIds, decimal? minPrice, decimal? maxPrice, bool onlyAvailable, ProductSort sort, int page, int limit);
    Task<(List<Category> categories, decimal minPrice, decimal maxPrice)> GetCatalogMetaAsync();
    Task<List<Product>> GetByIdsAsync(List<Guid> ids);
}