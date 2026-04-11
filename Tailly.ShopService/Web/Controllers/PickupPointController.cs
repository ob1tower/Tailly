using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ShopService.Application.Service.Interfaces;

namespace Tailly.ShopService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PickupPointController : ControllerBase
{
    private readonly IPickupPointService _pickupPointService;

    public PickupPointController(IPickupPointService pickupPointService)
    {
        _pickupPointService = pickupPointService;
    }

    /// <summary>
    /// Gets all pickup points. If 'city' query parameter is provided, returns only points from that city.
    /// </summary>
    /// <param name="city">Optional city name to filter pickup points.</param>
    /// <returns>List of pickup points.</returns>
    [HttpGet("pickup-points")]
    [EnableRateLimiting("pickup-points")]
    public async Task<IActionResult> GetPickupPoints([FromQuery] string? city = null)
    {
        if (!string.IsNullOrWhiteSpace(city))
        {
            var result = await _pickupPointService.GetByCityAsync(city);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        var allResult = await _pickupPointService.GetAllAsync();

        if (allResult.IsFailure)
            return BadRequest(allResult.Error);

        return Ok(allResult.Value);
    }

    /// <summary>
    /// Gets a single pickup point by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the pickup point.</param>
    [HttpGet("pickup-points/{id:guid}")]
    [EnableRateLimiting("pickup-points")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _pickupPointService.GetByIdAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }
}