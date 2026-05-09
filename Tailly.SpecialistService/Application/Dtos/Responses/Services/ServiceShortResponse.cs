namespace Tailly.SpecialistService.Application.Dtos.Responses.Services;

public sealed class ServiceShortResponse
{
    public string ServiceId { get; set; } = default!;
    public List<string> PetTypes { get; set; } = [];
    public decimal PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Note { get; set; }
}