using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Application.Dtos.Internal;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Web.Controllers.Internal;

[ApiController]
[Route("internal/calendar")]
[ApiExplorerSettings(IgnoreApi = true)]
public class InternalCalendarController : ControllerBase
{
    private readonly ICalendarRepository _repository;
    private readonly ICalendarService _service;

    public InternalCalendarController(ICalendarRepository repository,
                                      ICalendarService service)
    {
        _repository = repository;
        _service = service;
    }

    [HttpPost("check-availability")]
    public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityRequest request)
    {
        var calendar = await _repository.GetAsync(request.SpecialistId);

        if (calendar == null)
            return NotFound();

        var date = DateOnly.FromDateTime(request.StartAt);

        var startTime = TimeOnly.FromDateTime(request.StartAt);

        var endTime = TimeOnly.FromDateTime(request.EndAt);

        var dayOverride = calendar.DayOverrides
            .FirstOrDefault(x => x.Date == date);

        if (dayOverride != null)
        {
            if (dayOverride.Status == CalendarDayStatus.DayOff
                || dayOverride.Status == CalendarDayStatus.FullyBooked)
            {
                return BadRequest(new
                {
                    reason = "day_unavailable"
                });
            }
        }

        var hasWindow = calendar.AvailabilityWindows.Any(x =>
            x.Date == date
            && x.StartTime <= startTime
            && x.EndTime >= endTime);

        if (!hasWindow)
        {
            return BadRequest(new
            {
                reason = "outside_availability"
            });
        }

        var hasBookedIntersection = calendar.BookedSlots.Any(x =>
            x.Date == date
            && startTime < x.EndTime
            && endTime > x.StartTime);

        if (hasBookedIntersection)
        {
            return BadRequest(new
            {
                reason = "already_booked"
            });
        }

        return Ok(new
        {
            isAvailable = true
        });
    }

    [HttpPost("book-slot")]
    public async Task<IActionResult> BookSlot([FromBody] CreateBookedSlotRequest request)
    {
        var result = await _service.CreateBookedSlotAsync(
            request.SpecialistId,
            request.OrderId,
            request.ServiceId,
            request.StartAt,
            request.EndAt);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    [HttpDelete("booked-slots/order/{orderId:guid}")]
    public async Task<IActionResult> DeleteBookedSlot(Guid orderId)
    {
        var result = await _service.DeleteBookedSlotByOrderIdAsync(orderId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}