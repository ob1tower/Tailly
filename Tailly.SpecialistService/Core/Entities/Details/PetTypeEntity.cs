using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Entities.Details;

public class PetTypeEntity
{
    public Guid Id { get; set; }
    public Guid DetailsId { get; set; }
    public DetailsEntity Details { get; set; } = null!;
    public PetType PetType { get; set; }
}