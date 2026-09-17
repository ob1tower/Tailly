using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.SpecialistService.Application.Dtos.Requests.Services;
using Tailly.SpecialistService.Application.Mappers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Specialist")]
[EnableRateLimiting("specialist-actions")]
public class SpecialistServicesController : ControllerBase
{
    private readonly ISpecialistProfileService _service;
    private readonly IValidator<CreateServiceRequest> _createServiceValidator;
    private readonly IValidator<UpdateServiceRequest> _updateServiceValidator;

    public SpecialistServicesController(ISpecialistProfileService service,
                                       IValidator<CreateServiceRequest> createServiceValidator,
                                       IValidator<UpdateServiceRequest> updateServiceValidator)
    {
        _service = service;
        _createServiceValidator = createServiceValidator;
        _updateServiceValidator = updateServiceValidator;
    }

    /// <summary>
    /// Adds a new service to a specialist.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="request">The data of the new service.</param>
    [HttpPost("specialists/{slug}/services")]
    [Authorize(Roles = "Specialist")]
    public async Task<IActionResult> CreateService([FromRoute] string slug, [FromBody] CreateServiceRequest request)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var validationResult = await _createServiceValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var service = new ServiceOffer
        {
            Name = SpecialistEnumMapper.ParseServiceType(request.Name),
            Description = request.Description,
            Price = request.Price,
            PriceUnit = SpecialistEnumMapper.ParsePriceUnit(request.PriceUnit)
        };

        var result = await _service.AddServiceAsync(slug, specialistId.Value, service);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Updates an existing specialist service.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="serviceId">Service ID.</param>
    /// <param name="request">New service data.</param>
    [HttpPatch("specialists/{slug}/services/{serviceId:guid}")]
    [Authorize(Roles = "Specialist")]
    public async Task<IActionResult> UpdateService([FromRoute] string slug, [FromRoute] Guid serviceId, [FromBody] UpdateServiceRequest request)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var validationResult = await _updateServiceValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var service = new ServiceOffer
        {
            Id = serviceId,
            Name = SpecialistEnumMapper.ParseServiceType(request.Name),
            Description = request.Description,
            Price = request.Price,
            PriceUnit = SpecialistEnumMapper.ParsePriceUnit(request.PriceUnit)
        };

        var result = await _service.UpdateServiceAsync(slug, specialistId.Value, service);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Deletes the specialist's service.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="serviceId">Service ID.</param>
    [HttpDelete("specialists/{slug}/services/{serviceId:guid}")]
    [Authorize(Roles = "Specialist")]
    public async Task<IActionResult> DeleteService([FromRoute] string slug, [FromRoute] Guid serviceId)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var result = await _service.DeleteServiceAsync(slug, specialistId.Value, serviceId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}