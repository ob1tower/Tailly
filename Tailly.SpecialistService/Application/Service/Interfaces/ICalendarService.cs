using CSharpFunctionalExtensions;
using Tailly.SpecialistService.Core.Models.Calendars;

namespace Tailly.SpecialistService.Application.Service.Interfaces
{
    public interface ICalendarService
    {
        Task<Result> ClearDayAsync(string slug, Guid specialistId, string date);
        Task<Result> CreateAsync(string slug, Guid specialistId);
        Task<Result> CreateBookedSlotAsync(Guid specialistId, Guid orderId, Guid serviceId, DateTime startAt, DateTime endAt);
        Task<Result> DeleteAvailabilityWindowAsync(string slug, Guid specialistId, Guid windowId);
        Task<Result> DeleteBookedSlotByOrderIdAsync(Guid orderId);
        Task<Result> DeleteDayOverrideAsync(string slug, Guid specialistId, Guid overrideId);
        Task<Result> SaveAvailabilityWindowAsync(string slug, Guid specialistId, CalendarAvailabilityWindow window);
        Task<Result> SaveDayOverrideAsync(string slug, Guid specialistId, CalendarDayOverride dayOverride);
    }
}