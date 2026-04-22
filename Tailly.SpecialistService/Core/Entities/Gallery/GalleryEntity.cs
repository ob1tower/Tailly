using Tailly.SpecialistService.Core.Entities.Specialist;

namespace Tailly.SpecialistService.Core.Entities.Gallery;

public class GalleryEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = default!;
    public string ImageUrl { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}