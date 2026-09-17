namespace Tailly.SpecialistService.Core.Models.Gallery;

public class GalleryItem
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}