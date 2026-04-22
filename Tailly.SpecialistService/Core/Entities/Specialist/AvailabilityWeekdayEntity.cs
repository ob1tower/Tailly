namespace Tailly.SpecialistService.Core.Entities.Specialist;

public class AvailabilityWeekdayEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = default!;
    public DayOfWeek Weekday { get; set; }
}