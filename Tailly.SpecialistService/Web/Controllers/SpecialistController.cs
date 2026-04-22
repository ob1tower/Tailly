using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Application.Dtos.Requests.Specialist;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Mappers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Application.Validators;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpecialistController : ControllerBase
{
    private readonly ISpecialistsService _service;
    private readonly IValidator<GetSpecialistBySlugRequest> _validator;

    public SpecialistController(ISpecialistsService service,
                                IValidator<GetSpecialistBySlugRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>
    /// Gets specialists list.
    /// </summary>
    /// <param name="cityQuery">City filter.</param>
    /// <param name="districtQuery">District filter.</param>
    /// <param name="serviceId">Service ID filter.</param>
    /// <param name="priceMin">Minimum price.</param>
    /// <param name="priceMax">Maximum price.</param>
    /// <param name="experienceMinYears">Minimum experience.</param>
    /// <param name="hasReviewsOnly">Only specialists with reviews.</param>
    /// <param name="sort">Sorting: rating, price-asc, price-desc.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Items per page (default: 20).</param>
    [HttpGet("specialists")]
    public async Task<IActionResult> GetAll([FromQuery] string? cityQuery = null, [FromQuery] string? districtQuery = null, [FromQuery] string? serviceId = null, [FromQuery] decimal? priceMin = null, [FromQuery] decimal? priceMax = null,
                                            [FromQuery] int? experienceMinYears = null, [FromQuery] bool hasReviewsOnly = false, [FromQuery] string? sort = "rating", [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var pagination = new PaginationValidator(page, limit);

        var sortEnum = SpecialistEnumMapper.ParseSort(sort);

        var result = await _service.GetAllAsync(
            cityQuery,
            districtQuery,
            serviceId,
            priceMin,
            priceMax,
            experienceMinYears,
            hasReviewsOnly,
            sortEnum,
            pagination.PageNumber,
            pagination.PageSize);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (specialists, total) = result.Value;

        return Ok(
            specialists.Select(SpecialistMapper.ToShortResponse)
        );
    }

    /// <summary>
    /// Returns full specialist page by slug.
    /// </summary>
    /// <param name="slug">Specialist slug.</param>
    /// <returns>Specialist data</returns>
    [HttpGet("specialists/{slug}")]
    public async Task<IActionResult> GetBySlug([FromRoute] string slug)
    {
        var request = new GetSpecialistBySlugRequest
        {
            Slug = slug
        };

        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _service.GetBySlugAsync(request.Slug);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Specialist.NotFound" => NotFound(result.Error),
                "Specialist.InvalidSlug" => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        var response = result.Value.ToProfileResponse();

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
        var result = await _service.GetByIdAsync(id);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Specialist.NotFound" => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        var response = result.Value.ToProfileResponse();

        return Ok(response);
    }
}
