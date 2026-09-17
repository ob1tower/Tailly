using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Gallery;

namespace Tailly.SpecialistService.Core.Models.Specialist;

public class Details
{
    public HousingType HousingType { get; set; }
    public ChildrenPolicy HasChildrenUnderTen { get; set; }
    public string About { get; set; } = string.Empty;
    public List<PetSize> PetSizes { get; set; } = [];
    public List<PetAge> PetAges { get; set; } = [];
    public List<PetType> PetTypes { get; set; } = [];
    public List<GalleryItem> SpecialistGallery { get; set; } = [];
}