using Tailly.SpecialistService.Application.Dtos.Requests.Calendar;
using Tailly.SpecialistService.Application.Dtos.Responses.Calendar;
using Tailly.SpecialistService.Core.Models.Calendars;

namespace Tailly.SpecialistService.Application.Mappers;

public static class CalendarMapper
{
    public static CalendarResponse ToResponse(Calendar model)
    {
        return new CalendarResponse
        {
            Id = model.Id,

            AvailabilityWindows = model.AvailabilityWindows
                .Select(x => new AvailabilityWindowResponse
                {
                    Id = x.Id,
                    Date = x.Date.ToString("yyyy-MM-dd"),
                    StartTime = x.StartTime.ToString("HH:mm"),
                    EndTime = x.EndTime.ToString("HH:mm"),
                    ServiceId = x.ServiceId
                })
                .ToList(),

            DayOverrides = model.DayOverrides
                .Select(x => new DayOverrideResponse
                {
                    Id = x.Id,
                    Date = x.Date.ToString("yyyy-MM-dd"),
                    Status = SpecialistEnumMapper.MapCalendarDayStatus(x.Status)
                })
                .ToList(),

            BookedSlots = model.BookedSlots
                .Select(x => new BookedSlotResponse
                {
                    Id = x.Id,
                    Date = x.Date.ToString("yyyy-MM-dd"),
                    StartTime = x.StartTime.ToString("HH:mm"),
                    EndTime = x.EndTime.ToString("HH:mm"),
                    OrderId = x.OrderId,
                    ServiceId = x.ServiceId
                })
                .ToList()
        };
    }

    public static CalendarAvailabilityWindow ToModel(CreateAvailableSlotRequest request)
    {
        return new CalendarAvailabilityWindow
        {
            Date = DateOnly.Parse(request.Date),
            StartTime = TimeOnly.Parse(request.StartTime),
            EndTime = TimeOnly.Parse(request.EndTime),
            ServiceId = request.ServiceId
        };
    }

    public static CalendarDayOverride ToModel(CreateDayOverrideRequest request)
    {
        return new CalendarDayOverride
        {
            Date = DateOnly.Parse(request.Date),
            Status = SpecialistEnumMapper.ParseCalendarDayStatus(request.Status)
        };
    }
}