using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Entities.Details;

public class PetSizeEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = default!;
    public PetSize PetSize { get; set; }
}