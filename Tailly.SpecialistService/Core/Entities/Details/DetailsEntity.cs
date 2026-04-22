using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Entities.Details;

public class DetailsEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = default!;
    public HousingType HousingType { get; set; }
    public ChildrenPresence HasChildrenUnderTen { get; set; } 
    public string About { get; set; } = string.Empty;
    public string ExperienceLabel { get; set; } = string.Empty;
    public int? ExperienceDurationValue { get; set; }
    public ExperienceUnit? ExperienceDurationUnit { get; set; }
}
