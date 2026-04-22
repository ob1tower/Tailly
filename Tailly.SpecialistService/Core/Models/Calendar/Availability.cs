namespace Tailly.SpecialistService.Core.Models.Calendar;

public class Availability
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public List<Guid> ServiceIds { get; set; } = [];
}