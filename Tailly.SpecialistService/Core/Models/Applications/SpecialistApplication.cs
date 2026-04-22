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
    public string ServicesWanted { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }                      
    public SpecialistApplicationStatus Status { get; set; } = SpecialistApplicationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? InterviewNote { get; set; }
    public DateTime? InterviewDate { get; set; }
    public string? RejectionReason { get; set; }
}