using CSharpFunctionalExtensions;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Models.Pickup;

namespace Tailly.ShopService.Application.Service.Interfaces;

public interface IPickupPointService
{
    Task<Result<List<PickupPoint>, Error>> GetAllAsync();
    Task<Result<List<PickupPoint>, Error>> GetByCityAsync(string city);
    Task<Result<PickupPoint, Error>> GetByIdAsync(Guid id);
}