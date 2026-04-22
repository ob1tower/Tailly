using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Models.Specialist;

public class ServiceOffer
{
    public Guid? Id { get; set; }
    public Guid SpecialistId { get; set; }   
    public Specialist? Specialist { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PriceUnit PriceUnit { get; set; }
    public string LocationLabel { get; set; } = string.Empty;
    public ServiceType Type { get; set; }
}