using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.SpecialistService.Application.Dtos.Requests.Calendar;
using Tailly.SpecialistService.Application.Mappers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Specialist")]
[EnableRateLimiting("specialist-actions")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;
    private readonly IValidator<CreateAvailableSlotRequest> _slotValidator;
    private readonly IValidator<CreateDayOverrideRequest> _dayOverrideValidator;

    public CalendarController(ICalendarService calendarService,
                              IValidator<CreateAvailableSlotRequest> slotValidator,
                              IValidator<CreateDayOverrideRequest> dayOverrideValidator)
    {
        _calendarService = calendarService;
        _slotValidator = slotValidator;
        _dayOverrideValidator = dayOverrideValidator;
    }

    /// <summary>
    /// Creates available calendar slot for specialist.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="request">Slot data.</param>
    [HttpPost("specialists/{slug}/slots")]
    public async Task<IActionResult> CreateSlot([FromRoute] string slug,
                                                [FromBody] CreateAvailableSlotRequest request)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var validationResult = await _slotValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var model = CalendarMapper.ToModel(request);

        var result = await _calendarService.SaveAvailabilityWindowAsync(
            slug,
            specialistId.Value,
            model);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Deletes available calendar slot.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="slotId">Slot ID.</param>
    [HttpDelete("specialists/{slug}/slots/{slotId:guid}")]
    public async Task<IActionResult> DeleteSlot([FromRoute] string slug,
                                                [FromRoute] Guid slotId)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var result = await _calendarService.DeleteAvailabilityWindowAsync(
            slug,
            specialistId.Value,
            slotId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Creates calendar day override.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="request">Day override data.</param>
    [HttpPost("specialists/{slug}/day-overrides")]
    public async Task<IActionResult> CreateDayOverride([FromRoute] string slug,
                                                       [FromBody] CreateDayOverrideRequest request)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var validationResult = await _dayOverrideValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var model = CalendarMapper.ToModel(request);

        var result = await _calendarService.SaveDayOverrideAsync(
            slug,
            specialistId.Value,
            model);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Deletes calendar day override.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="overrideId">Day override ID.</param>
    [HttpDelete("specialists/{slug}/day-overrides/{overrideId:guid}")]
    public async Task<IActionResult> DeleteDayOverride([FromRoute] string slug,
                                                       [FromRoute] Guid overrideId)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var result = await _calendarService.DeleteDayOverrideAsync(
            slug,
            specialistId.Value,
            overrideId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Clears specialist calendar day.
    /// </summary>
    /// <param name="slug">Specialist's slug.</param>
    /// <param name="date">Date in yyyy-MM-dd format.</param>
    [HttpDelete("specialists/{slug}/days/{date}")]
    public async Task<IActionResult> ClearDay([FromRoute] string slug,
                                              [FromRoute] string date)
    {
        var specialistId = User.GetSpecialistId();

        if (specialistId == null)
            return Unauthorized();

        var result = await _calendarService.ClearDayAsync(
            slug,
            specialistId.Value,
            date);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}
