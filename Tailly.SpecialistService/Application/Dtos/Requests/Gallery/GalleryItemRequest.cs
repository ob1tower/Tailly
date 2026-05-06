namespace Tailly.SpecialistService.Application.Dtos.Requests.Gallery;

public sealed class GalleryItemRequest
{
    public string ImageUrl { get; set; } = default!;
    public string? Alt { get; set; }
}