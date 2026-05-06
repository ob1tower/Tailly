using Tailly.SpecialistService.Application.Dtos.Requests.Gallery;

namespace Tailly.SpecialistService.Application.Dtos.Requests.Specialist;

public sealed class UpdateSpecialistDetailsRequest
{
    public string HousingType { get; set; } = default!;
    public string HasChildrenUnderTen { get; set; } = default!;
    public string About { get; set; } = default!;
    public List<string> PetSizes { get; set; } = [];
    public List<string> PetAges { get; set; } = [];
    public List<string> PetTypes { get; set; } = [];
    public List<GalleryItemRequest> SpecialistGallery { get; set; } = [];
}