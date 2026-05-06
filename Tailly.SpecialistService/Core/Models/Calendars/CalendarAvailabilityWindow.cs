namespace Tailly.SpecialistService.Core.Models.Calendars;

public class CalendarAvailabilityWindow
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Comment { get; set; }
}