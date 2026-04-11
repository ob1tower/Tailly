using Tailly.ShopService.Core.Models.Order;

namespace Tailly.ShopService.Infrastructure.Repositories.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task CancelAsync(Guid orderId);
    Task<bool> ExistsAsync(Guid id);
    Task<Order?> GetByIdAsync(Guid id);
    Task<List<Order>> GetByUserIdAsync(Guid userId);
    Task UpdateAsync(Order order);
}