using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Models.Calendars;

public class CalendarDayOverride
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public DateOnly Date { get; set; }
    public CalendarDayStatus Status { get; set; }
}