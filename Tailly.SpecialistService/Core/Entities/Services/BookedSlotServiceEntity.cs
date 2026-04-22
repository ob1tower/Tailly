using Tailly.SpecialistService.Core.Entities.Calendar;

namespace Tailly.SpecialistService.Core.Entities.Services;

public class BookedSlotServiceEntity
{
    public Guid Id { get; set; }
    public Guid BookedSlotId { get; set; }
    public BookedSlotEntity BookedSlot { get; set; } = default!;
    public Guid ServiceId { get; set; }
    public ServiceEntity Service { get; set; } = default!;
}