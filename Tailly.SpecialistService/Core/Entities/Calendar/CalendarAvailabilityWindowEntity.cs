namespace Tailly.SpecialistService.Core.Entities.Calendar;

public class CalendarAvailabilityWindowEntity
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public CalendarEntity Calendar { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid? ServiceId { get; set; }
}