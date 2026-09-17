using CSharpFunctionalExtensions;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Catalog;
using Tailly.ShopService.Core.Models.Products;

namespace Tailly.ShopService.Application.Service.Interfaces;

public interface IProductService
{
    Task<Result> CreateReviewAsync(Guid userId, ProductReview review);
    Task<Result<List<Product>, Error>> GetByIdsAsync(string ids);
    Task<Result<Product, Error>> GetBySlugAsync(string slug, ReviewSortType reviewSort);
    Task<Result<(List<Product> products, int total), Error>> GetCatalogAsync(CatalogFilterState filter);
    Task<Result<(List<Category> categories, decimal minPrice, decimal maxPrice), Error>> GetCatalogMetaAsync();
    Task<Result> ReplyToReviewAsync(Guid reviewId, string replyText);
}