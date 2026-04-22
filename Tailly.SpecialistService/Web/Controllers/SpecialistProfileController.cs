using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistProfile;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Mappers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Specialist")]
[ApiController]
public class SpecialistProfileController : ControllerBase
{
    private readonly ISpecialistsService _service;
    private readonly IValidator<SpecialistMainInfoUpdateRequest> _validator;
    private readonly IValidator<SpecialistDetailsUpdateRequest> _detailsValidator;

    public SpecialistProfileController(ISpecialistsService service,
                                       IValidator<SpecialistMainInfoUpdateRequest> validator,
                                       IValidator<SpecialistDetailsUpdateRequest> detailsValidator)
    {
        _service = service;
        _validator = validator;
        _detailsValidator = detailsValidator;
    }

    [HttpPatch("specialists/{slug}/main")]
    public async Task<IActionResult> UpdateMain(string slug, [FromBody] SpecialistMainInfoUpdateRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _service.UpdateMainAsync(
            slug,
            userId.Value,
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.City,
            request.District,
            request.Phone,
            request.AvatarUrl);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }

    [HttpPatch("specialists/{slug}/details")]
    public async Task<IActionResult> UpdateDetails(string slug, [FromBody] SpecialistDetailsUpdateRequest request)
    {
        var validation = await _detailsValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var experienceUnit = SpecialistEnumMapper.ParseExperienceUnit(request.ExperienceDurationUnit);
        var housingType = SpecialistEnumMapper.ParseHousingType(request.HousingType);
        var childrenPresence = SpecialistEnumMapper.ParseChildrenPresence(request.HasChildrenUnderTen);

        var petTypes = request.PetTypes.Select(SpecialistEnumMapper.ParsePetType).ToList();
        var petSizes = request.PetSizes.Select(SpecialistEnumMapper.ParsePetSize).ToList();
        var petAges = request.PetAges.Select(SpecialistEnumMapper.ParsePetAge).ToList();

        var advantages = request.Advantages
            .Select(title => new Advantage { Title = title })
            .ToList();

        var services = request.Services.Select(s => new ServiceOffer
        {
            Id = s.Id,
            Name = s.Name,
            LocationLabel = s.LocationLabel,
            Price = s.Price,
            PriceUnit = SpecialistEnumMapper.ParsePriceUnit(s.PriceUnit),
            Type = SpecialistEnumMapper.ParseServiceType(s.Name)
        }).ToList();

        var result = await _service.UpdateDetailsAsync(
            slug,
            userId.Value,
            request.ExperienceLabel,
            request.ExperienceDurationValue,
            experienceUnit,
            housingType,
            petSizes,
            petAges,
            childrenPresence,
            petTypes,
            advantages,
            request.About,
            services);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Specialist.NotFound" => NotFound(result.Error),
                "Specialist.Forbidden" => Forbid(),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new { success = true });
    }
}