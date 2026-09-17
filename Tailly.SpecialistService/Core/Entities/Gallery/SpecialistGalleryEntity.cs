using Tailly.SpecialistService.Core.Entities.Specialist;

namespace Tailly.SpecialistService.Core.Entities.Gallery;

public class SpecialistGalleryEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = null!;
    public int Order { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}