using CSharpFunctionalExtensions;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Models.Pickup;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Application.Service;

public class PickupPointService : IPickupPointService
{
    private readonly IPickupPointRepository _pickupPointRepository;
    private readonly ILogger<PickupPointService> _logger;

    public PickupPointService(IPickupPointRepository pickupPointRepository,
                              ILogger<PickupPointService> logger)
    {
        _pickupPointRepository = pickupPointRepository;
        _logger = logger;
    }

    public async Task<Result<List<PickupPoint>, Error>> GetAllAsync()
    {
        var pickupPoints = await _pickupPointRepository.GetAllAsync();

        _logger.LogInformation("Retrieved {Count} pickup points from database", pickupPoints.Count);

        return Result.Success<List<PickupPoint>, Error>(pickupPoints);
    }

    public async Task<Result<List<PickupPoint>, Error>> GetByCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            _logger.LogWarning("GetByCityAsync failed: city name is empty or whitespace");
            return Result.Failure<List<PickupPoint>, Error>(ShopErrors.InvalidCityName);
        }

        var trimmedCity = city.Trim();

        var pickupPoints = await _pickupPointRepository.GetByCityAsync(trimmedCity);

        _logger.LogInformation("Retrieved {Count} pickup points for city: '{City}'", pickupPoints.Count, trimmedCity);

        return Result.Success<List<PickupPoint>, Error>(pickupPoints);
    }

    public async Task<Result<PickupPoint, Error>> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("GetByIdAsync failed: empty PickupPoint Id provided");
            return Result.Failure<PickupPoint, Error>(ShopErrors.PickupPointNotFound);
        }

        var pickupPoint = await _pickupPointRepository.GetByIdAsync(id);

        if (pickupPoint == null)
        {
            _logger.LogWarning("Pickup point not found by Id: {PickupPointId}", id);
            return Result.Failure<PickupPoint, Error>(ShopErrors.PickupPointNotFound);
        }

        _logger.LogInformation("Pickup point retrieved successfully. Id: {PickupPointId}, Title: {Title}",
            id, pickupPoint.Title);

        return Result.Success<PickupPoint, Error>(pickupPoint);
    }
}