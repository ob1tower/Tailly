namespace Tailly.SpecialistService.Core.Models.Calendars;

public class Calendar
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public List<CalendarDayOverride> DayOverrides { get; set; } = [];
    public List<CalendarBookedSlot> BookedSlots { get; set; } = [];
    public List<CalendarAvailabilityWindow> AvailabilityWindows { get; set; } = [];
}