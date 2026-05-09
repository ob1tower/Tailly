using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.SpecialistService.Application.Dtos.Requests.Reviews;
using Tailly.SpecialistService.Application.Dtos.Requests.Services;
using Tailly.SpecialistService.Application.Dtos.Requests.Specialist;
using Tailly.SpecialistService.Application.Dtos.Responses.Options;
using Tailly.SpecialistService.Application.Mappers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("specialist-actions")]
public class SpecialistProfileController : ControllerBase
{
    private readonly ISpecialistProfileService _service;
    private readonly IValidator<UpdateSpecialistMainInfoRequest> _updateMainValidator;
    private readonly IValidator<UpdateSpecialistDetailsRequest> _updateDetailsValidator;
    private readonly IValidator<CreateServiceRequest> _createServiceValidator;
    private readonly IValidator<UpdateServiceRequest> _updateServiceValidator;
    private readonly IValidator<ReviewReplyRequest> _replyValidator;

    public SpecialistProfileController(ISpecialistProfileService service,
                                       IValidator<UpdateSpecialistMainInfoRequest> updateMainValidator,
                                       IValidator<UpdateSpecialistDetailsRequest> updateDetailsValidator,
                                       IValidator<CreateServiceRequest> createServiceValidator,
                                       IValidator<UpdateServiceRequest> updateServiceValidator,
                                       IValidator<ReviewReplyRequest> replyValidator)
    {
        _service = service;
        _updateMainValidator = updateMainValidator;
        _updateDetailsValidator = updateDetailsValidator;
        _createServiceValidator = createServiceValidator;
        _updateServiceValidator = updateServiceValidator;
        _replyValidator = replyValidator;
    }

    /// <summary>
    /// Updates the specialist's basic data (fullName, city, phoneNumber, avatar).
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="request">Data to update.</param>
    [HttpPatch("specialists/{slug}/main")]
    [Authorize(Roles = "Specialist")]
    public async Task<IActionResult> UpdateMain([FromRoute] string slug, [FromBody] UpdateSpecialistMainInfoRequest request)
    {
        var userId = User.GetUserId();
        var specialistId = User.GetSpecialistId();

        if (userId == null || specialistId == null)
            return Unauthorized();

        var validationResult = await _updateMainValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var result = await _service.UpdateMainInfoAsync(
            slug: slug,
            specialistId: specialistId.Value,
            userId: userId.Value,
            firstName: request.FirstName,
            lastName: request.LastName,
            middleName: request.MiddleName,
            city: request.City,
            district: request.District,
            phone: request.Phone,
            avatarUrl: request.AvatarUrl);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Updates the details of the specialist's profile (housing, pets, about yourself, gallery).
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="request">Data for updating details.</param>
    [HttpPatch("specialists/{slug}/details")]
    [Authorize(Roles = "Specialist")]
    public async Task<IActionResult> UpdateDetails([FromRoute] string slug, [FromBody] UpdateSpecialistDetailsRequest request)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var validationResult = await _updateDetailsValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var details = SpecialistResponseMapper.ToDetails(request);

        var result = await _service.UpdateDetailsAsync(slug, specialistId.Value, details);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
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

    /// <summary>
    /// Adds or updates the specialist's response to the review.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="reviewId">Review ID.</param>
    /// <param name="request">Response text.</param>
    [HttpPut("specialists/{slug}/reviews/{reviewId:guid}/reply")]
    [Authorize(Roles = "Specialist")]
    public async Task<IActionResult> ReplyToReview([FromRoute] string slug, [FromRoute] Guid reviewId, [FromBody] ReviewReplyRequest request)
    {
        var specialistId = User.GetSpecialistId();
        if (specialistId == null)
            return Unauthorized();

        var validationResult = await _replyValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var result = await _service.ReplyToReviewAsync(slug, specialistId.Value, reviewId, request.Text);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Returns directories for editing the specialist's profile 
    /// (housing types, pet sizes, ages, types of services, etc.).
    /// </summary>
    [HttpGet("specialists/{slug}/edit-options")]
    [AllowAnonymous]
    public IActionResult GetEditOptions(string slug)
    {
        var response = new SpecialistProfileEditOptionsResponse
        {
            HousingTypes = Enum.GetNames(typeof(HousingType)).ToList(),
            PetTypes = Enum.GetNames(typeof(PetType)).ToList(),
            PetSizes = Enum.GetNames(typeof(PetSize)).ToList(),
            PetAges = Enum.GetNames(typeof(PetAge)).ToList(),
            ChildrenPresences = Enum.GetNames(typeof(ChildrenPolicy)).ToList(),
            PriceUnits = Enum.GetNames(typeof(ServicePriceUnit)).ToList(),
            ExperienceUnits = Enum.GetNames(typeof(ExperienceUnit)).ToList()
        };

        return Ok(response);
    }
}