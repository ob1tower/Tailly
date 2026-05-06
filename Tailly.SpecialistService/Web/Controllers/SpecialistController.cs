using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Application.Validators;
using static Tailly.SpecialistService.Application.Mappers.SpecialistResponseMapper;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpecialistController : ControllerBase
{
    private readonly ISpecialistsService _service;

    public SpecialistController(ISpecialistsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gets specialists list.
    /// </summary>
    /// <param name="cityQuery">City filter.</param>
    /// <param name="districtQuery">District filter.</param>
    /// <param name="serviceType">Service type filter (walking, boarding, grooming, training, photoshoot).</param>
    /// <param name="priceMin">Minimum price.</param>
    /// <param name="priceMax">Maximum price.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Items per page (default: 20).</param>
    [HttpGet("specialists")]
    public async Task<IActionResult> GetAll([FromQuery] string? cityQuery = null, [FromQuery] string? districtQuery = null, [FromQuery] string? serviceType = null, [FromQuery] decimal? priceMin = null, [FromQuery] decimal? priceMax = null, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var pagination = new PaginationValidator(page, limit);

        var result = await _service.SearchAsync(
            cityQuery,
            districtQuery,
            serviceType,
            priceMin,
            priceMax,
            pagination.PageNumber,
            pagination.PageSize);

        return Ok(result.Select(ToShortResponse));
    }

    /// <summary>
    /// Returns full specialist page by slug.
    /// </summary>
    /// <param name="slug">Specialist slug.</param>
    /// <returns>Specialist data</returns>
    [HttpGet("specialists/{slug}")]
    public async Task<IActionResult> GetBySlug([FromRoute] string slug)
    {
        var specialist = await _service.GetFullProfileBySlugAsync(slug);

        if (specialist == null)
            return NotFound();

        var response = ToResponse(specialist);
        return Ok(response);
    }

    /// <summary>
    /// Returns full specialist page by ID.
    /// </summary>
    /// <param name="id">Specialist ID.</param>
    /// <returns>Specialist data.</returns>
    [HttpGet("specialists/{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var specialist = await _service.GetFullProfileByIdAsync(id);

        if (specialist == null)
            return NotFound();

        var response = ToResponse(specialist);
        return Ok(response);
    }
}