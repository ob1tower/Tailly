namespace Tailly.SpecialistService.Application.Dtos.Responses.SpecialistApplication;

public sealed class SpecialistApplicationListItemResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;                  
    public string City { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }                       
    public string? InterviewDate { get; set; }
    public string? ReviewComment { get; set; }                     
    public string? ReviewedBy { get; set; }                        
    public string About { get; set; } = default!;
    public Guid? CreatedSpecialistId { get; set; }                 
    public string? CreatedSpecialistSlug { get; set; }              
    public DateTime? SpecialistAccountCreatedAt { get; set; }             
    public SpecialistApplicationQuestionnaireResponse? Questionnaire { get; set; }
}