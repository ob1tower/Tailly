using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Entities.Calendar;

public class CalendarDayOverrideEntity
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public CalendarEntity Calendar { get; set; } = null!;
    public DateOnly Date { get; set; }
    public CalendarDayStatus Status { get; set; }
}