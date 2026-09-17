namespace Tailly.SpecialistService.Core.Entities.Calendar;

public class CalendarBookedSlotEntity
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public CalendarEntity Calendar { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? ServiceId { get; set; }
}