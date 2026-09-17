namespace Tailly.SpecialistService.Application.Dtos.Responses.Specialist;

public sealed class SpecialistDetailsResponse
{
    public string HousingType { get; set; } = default!;
    public string HasChildrenUnderTen { get; set; } = default!;
    public string About { get; set; } = default!;
    public List<string> PetSizes { get; set; } = [];
    public List<string> PetAges { get; set; } = [];
    public List<string> PetTypes { get; set; } = [];
}