using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Models.Specialist;

public class ServiceOffer
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public ServiceType Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public ServicePriceUnit PriceUnit { get; set; }
}