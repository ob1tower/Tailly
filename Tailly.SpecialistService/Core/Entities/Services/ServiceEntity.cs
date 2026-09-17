using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Entities.Services;

public class ServiceEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = null!;
    public ServiceType Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public ServicePriceUnit PriceUnit { get; set; }
}