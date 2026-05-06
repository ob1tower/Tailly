namespace Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

public sealed class SpecialistApplicationQuestionnaire
{
    public int ExperienceYears { get; set; }
    public List<string> AnimalTypes { get; set; } = [];
    public List<string> ServiceFormats { get; set; } = [];
    public bool CanGiveMedication { get; set; }
    public bool CanHandleDifficultBehavior { get; set; }
    public bool CanTakeOvernightOrders { get; set; }
    public bool HasOwnPets { get; set; }
    public bool HasPetFirstAidBasics { get; set; }
    public string HousingType { get; set; } = default!;
    public string DistrictPreferences { get; set; } = default!;
    public string SchedulePreferences { get; set; } = default!;
    public string PortfolioUrl { get; set; } = default!;
    public string Motivation { get; set; } = default!;
    public string AdditionalInfo { get; set; } = default!;
}