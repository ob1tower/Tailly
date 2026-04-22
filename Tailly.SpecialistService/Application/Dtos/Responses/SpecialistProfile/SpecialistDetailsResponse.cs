namespace Tailly.SpecialistService.Application.Dtos.Responses.SpecialistProfile;

public sealed class SpecialistDetailsResponse
{
    public string ExperienceLabel { get; set; } = default!;
    public int? ExperienceDurationValue { get; set; }
    public string? ExperienceDurationUnit { get; set; }
    public string HousingType { get; set; } = default!;
    public string HasChildrenUnderTen { get; set; } = default!;
    public List<string> PetTypes { get; set; } = [];
    public List<string> PetSizes { get; set; } = [];
    public List<string> PetAges { get; set; } = [];
    public List<string> Advantages { get; set; } = [];
    public string About { get; set; } = default!;
}