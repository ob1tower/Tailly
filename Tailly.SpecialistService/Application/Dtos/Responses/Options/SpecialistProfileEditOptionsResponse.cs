namespace Tailly.SpecialistService.Application.Dtos.Responses.Options;

public sealed class SpecialistProfileEditOptionsResponse
{
    public List<string> HousingTypes { get; set; } = new();
    public List<string> PetTypes { get; set; } = new();
    public List<string> PetSizes { get; set; } = new();
    public List<string> PetAges { get; set; } = new();
    public List<string> ChildrenPresences { get; set; } = new();
    public List<string> PriceUnits { get; set; } = new();
    public List<string> ExperienceUnits { get; set; } = new();
}