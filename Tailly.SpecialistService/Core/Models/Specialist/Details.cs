using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Models.Specialist;

public class Details
{
    public HousingType HousingType { get; set; }
    public ChildrenPresence HasChildrenUnderTen { get; set; }
    public string About { get; set; } = string.Empty;
    public string ExperienceLabel { get; set; } = string.Empty;
    public int? ExperienceDurationValue { get; set; }
    public ExperienceUnit? ExperienceDurationUnit { get; set; }
    public List<PetType> PetTypes { get; set; } = [];
    public List<PetSize> PetSizes { get; set; } = [];
    public List<PetAge> PetAges { get; set; } = [];
    public List<string> Advantages { get; set; } = [];
}
