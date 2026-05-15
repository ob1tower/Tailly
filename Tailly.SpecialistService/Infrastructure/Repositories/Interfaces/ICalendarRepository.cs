using Tailly.SpecialistService.Core.Models.Calendars;

namespace Tailly.SpecialistService.Infrastructure.Repositories.Interfaces
{
    public interface ICalendarRepository
    {
        Task<bool> AvailabilityWindowBelongsToSpecialistAsync(Guid specialistId, Guid windowId);
        Task<bool> AvailabilityWindowExistsAsync(Guid windowId);
        Task CreateAsync(Guid specialistId);
        Task CreateBookedSlotAsync(Guid specialistId, CalendarBookedSlot slot);
        Task<bool> DayOverrideBelongsToSpecialistAsync(Guid specialistId, Guid overrideId);
        Task<bool> DayOverrideExistsAsync(Guid overrideId);
        Task<bool> DayOverrideExistsForDateAsync(Guid specialistId, DateOnly date, Guid? excludeId = null);
        Task DeleteAvailabilityWindowAsync(Guid specialistId, Guid windowId);
        Task<bool> DeleteBookedSlotByOrderIdAsync(Guid orderId);
        Task DeleteDayOverrideAsync(Guid specialistId, Guid overrideId);
        Task<Calendar?> GetAsync(Guid specialistId);
        Task<bool> HasAvailabilityIntersectionAsync(Guid specialistId, CalendarAvailabilityWindow window);
        Task SaveAvailabilityWindowAsync(Guid specialistId, CalendarAvailabilityWindow window);
        Task SaveDayOverrideAsync(Guid specialistId, CalendarDayOverride dayOverride);
        Task DeleteAllAvailabilityWindowsForDateAsync(Guid specialistId, DateOnly date);
        Task ClearDayAsync(Guid specialistId, DateOnly date);
    }
}