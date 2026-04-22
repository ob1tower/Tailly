using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Mappers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpecialistApplicationController : ControllerBase
{
    private readonly ISpecialistApplicationService _service;
    private readonly IValidator<CreateSpecialistApplicationRequest> _validator;

    public SpecialistApplicationController(ISpecialistApplicationService service,
                                           IValidator<CreateSpecialistApplicationRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>
    /// Submits a new specialist application.
    /// Can be called by both authenticated clients and guests.
    /// </summary>
    /// <param name="request">Specialist application data.</param>
    /// <returns>Application ID.</returns>
    [HttpPost("specialist-applications")]
    public async Task<IActionResult> Create([FromBody] CreateSpecialistApplicationRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId() ?? Guid.Empty;

        var application = SpecialistApplicationMapper.ToModel(request, userId);

        var result = await _service.CreateAsync(application);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { id = result.Value.Id });
    }
}