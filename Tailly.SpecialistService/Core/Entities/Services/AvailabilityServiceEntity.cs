using Tailly.SpecialistService.Core.Entities.Calendar;

namespace Tailly.SpecialistService.Core.Entities.Services;

public class AvailabilityServiceEntity
{
    public Guid Id { get; set; }
    public Guid AvailabilityId { get; set; }
    public AvailabilityEntity Availability { get; set; } = default!;
    public Guid ServiceId { get; set; }
    public ServiceEntity Service { get; set; } = default!;
}