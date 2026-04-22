namespace Tailly.SpecialistService.Application.Dtos.Requests.SpecialistProfile;

public sealed class SpecialistDetailsUpdateRequest
{
    public string ExperienceLabel { get; set; } = default!;
    public int? ExperienceDurationValue { get; set; }
    public string? ExperienceDurationUnit { get; set; }   
    public string HousingType { get; set; } = default!;
    public List<string> PetSizes { get; set; } = [];
    public List<string> PetAges { get; set; } = [];
    public string HasChildrenUnderTen { get; set; } = "no";
    public List<string> PetTypes { get; set; } = [];
    public List<string> Advantages { get; set; } = [];
    public string About { get; set; } = string.Empty;
    public List<SpecialistServiceUpdateItem> Services { get; set; } = [];
}