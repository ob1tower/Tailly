using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Core.Models.Applications;

public class SpecialistApplication
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string AnimalTypes { get; set; } = string.Empty;
    public string ServiceFormats { get; set; } = string.Empty;
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
    public string? PhotoUrl { get; set; }
    public SpecialistApplicationStatus Status { get; set; } = SpecialistApplicationStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? ReviewComment { get; set; }
    public string? ReviewedBy { get; set; }
    public string? InterviewNote { get; set; }
    public DateTime? InterviewDate { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? CreatedSpecialistId { get; set; }
    public string? CreatedSpecialistSlug { get; set; }
    public DateTime? SpecialistAccountCreatedAt { get; set; }
}