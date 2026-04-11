using Tailly.ShopService.Core.Models.Cart;

namespace Tailly.ShopService.Infrastructure.Repositories.Interfaces;

public interface ICartRepository
{
    Task AddItemAsync(Guid cartId, CartItem item);
    Task ClearAsync(Guid cartId);
    Task<Cart> CreateAsync(Guid? userId, Guid? sessionId);
    Task<Cart?> GetBySessionIdAsync(Guid sessionId);
    Task<Cart?> GetByUserIdAsync(Guid userId);
    Task RemoveItemAsync(Guid cartId, Guid productId);
    Task UpdateItemAsync(Guid cartId, Guid productId, int quantity);
    Task AssignUserAsync(Guid cartId, Guid userId);
    Task MergeAsync(Guid targetCartId, Cart sourceCart);
    Task DeleteBySessionIdAsync(Guid sessionId);
}