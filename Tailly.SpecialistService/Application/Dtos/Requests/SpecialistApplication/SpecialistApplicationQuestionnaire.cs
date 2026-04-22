namespace Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

public sealed class SpecialistApplicationQuestionnaire
{
    public string ExperienceYears { get; set; } = string.Empty;   
    public List<string> AnimalTypes { get; set; } = [];
    public List<string> ServiceFormats { get; set; } = [];
    public bool CanGiveMedication { get; set; }
    public bool CanHandleDifficultBehavior { get; set; }
    public bool CanTakeOvernightOrders { get; set; }
    public bool HasOwnPets { get; set; }
    public bool HasPetFirstAidBasics { get; set; }
    public string HousingType { get; set; } = string.Empty;
    public string DistrictPreferences { get; set; } = string.Empty;
    public string SchedulePreferences { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
    public string Motivation { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = string.Empty;
}