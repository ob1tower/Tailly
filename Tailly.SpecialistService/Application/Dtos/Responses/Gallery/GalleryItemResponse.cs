namespace Tailly.SpecialistService.Application.Dtos.Responses.Gallery;

public sealed class GalleryItemResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = default!;
    public string Alt { get; set; } = default!;
    public int Order { get; set; }
}