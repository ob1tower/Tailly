using Tailly.SpecialistService.Core.Entities.Calendar;
using Tailly.SpecialistService.Core.Models.Calendars;

namespace Tailly.SpecialistService.Infrastructure.Mappers;

public static class CalendarEntityMapper
{
    public static Calendar ToModel(CalendarEntity entity)
    {
        return new Calendar
        {
            Id = entity.Id,
            SpecialistId = entity.SpecialistId,

            DayOverrides = entity.DayOverrides?
                .Select(x => new CalendarDayOverride
                {
                    Id = x.Id,
                    Date = x.Date,
                    Status = x.Status
                })
                .ToList() ?? [],

            BookedSlots = entity.BookedSlots?
                .Select(x => new CalendarBookedSlot
                {
                    Id = x.Id,
                    Date = x.Date,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    OrderId = x.OrderId,
                    ServiceId = x.ServiceId
                })
                .ToList() ?? [],

            AvailabilityWindows = entity.AvailabilityWindows?
                .Select(x => new CalendarAvailabilityWindow
                {
                    Id = x.Id,
                    Date = x.Date,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    ServiceId = x.ServiceId
                })
                .ToList() ?? []
        };
    }
}