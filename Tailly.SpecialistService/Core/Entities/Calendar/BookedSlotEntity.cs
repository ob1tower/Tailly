using Tailly.SpecialistService.Core.Entities.Services;
using Tailly.SpecialistService.Core.Entities.Specialist;

namespace Tailly.SpecialistService.Core.Entities.Calendar;

public class BookedSlotEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = default!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid? OrderId { get; set; }
    public ICollection<BookedSlotServiceEntity> Services { get; set; } = [];
}