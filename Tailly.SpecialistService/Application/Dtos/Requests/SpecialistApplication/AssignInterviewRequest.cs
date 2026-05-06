namespace Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

public sealed class AssignInterviewRequest
{
    public string Note { get; set; } = default!;
    public DateTime? InterviewDate { get; set; }
    public string? ReviewedBy { get; set; }
}