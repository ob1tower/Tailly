namespace Tailly.ClientProfileService.Application.Dtos.Requests;

public sealed class UpsertPetRequest
{
    public string Name { get; set; } = default!;
    public string? PhotoUrl { get; set; }
    public string? Type { get; set; }
    public string? BreedId { get; set; }
    public int AgeYears { get; set; }
    public int AgeMonths { get; set; }
    public string? Size { get; set; }
    public string? Gender { get; set; }
    public string? ToOtherPets { get; set; }
    public string? ToKidsUnder10 { get; set; }
    public string? StaysHomeAlone { get; set; }
    public string? Vaccinated { get; set; }
    public string? Notes { get; set; }
}