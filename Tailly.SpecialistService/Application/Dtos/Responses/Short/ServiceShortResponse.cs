namespace Tailly.SpecialistService.Application.Dtos.Responses.Short;

public sealed class ServiceShortResponse
{
    public string ServiceId { get; set; } = default!;
    public List<string> PetTypes { get; set; } = [];
    public decimal PriceFrom { get; set; }
}