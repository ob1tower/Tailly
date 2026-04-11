using Tailly.ShopService.Core.Models.Pickup;

namespace Tailly.ShopService.Infrastructure.Repositories.Interfaces;

public interface IPickupPointRepository
{
    Task<bool> ExistsAsync(Guid id);
    Task<List<PickupPoint>> GetAllAsync();
    Task<List<PickupPoint>> GetByCityAsync(string city);
    Task<PickupPoint?> GetByIdAsync(Guid id);
}