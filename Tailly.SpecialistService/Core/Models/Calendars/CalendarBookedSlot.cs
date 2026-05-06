namespace Tailly.SpecialistService.Core.Models.Calendars;

public class CalendarBookedSlot
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid? OrderId { get; set; }
}