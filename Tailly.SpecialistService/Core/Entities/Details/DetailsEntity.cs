using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Entities.Details;

public class DetailsEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = null!;
    public HousingType HousingType { get; set; }
    public ChildrenPolicy HasChildrenUnderTen { get; set; }
    public string About { get; set; } = string.Empty;
    public ICollection<PetSizeEntity> PetSizes { get; set; } = new List<PetSizeEntity>();
    public ICollection<PetAgeEntity> PetAges { get; set; } = new List<PetAgeEntity>();
    public ICollection<PetTypeEntity> PetTypes { get; set; } = new List<PetTypeEntity>();
}