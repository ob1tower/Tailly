using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Core.Models.User;

namespace Tailly.ShopService.Infrastructure.Repositories.Interfaces;

public interface IFavoriteRepository
{
    Task AddAsync(Favorite favorite);
    Task ClearAsync(Guid? userId, Guid? sessionId);
    Task<bool> ExistsAsync(Guid? userId, Guid? sessionId, Guid productId);
    Task<List<Favorite>> GetAsync(Guid? userId, Guid? sessionId);
    Task RemoveAsync(Guid? userId, Guid? sessionId, Guid productId);
    Task<List<Product>> GetByIdsAsync(List<Guid> ids);
}