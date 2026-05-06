namespace Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

public sealed class ApproveApplicationRequest
{
    public string? ReviewComment { get; set; }
    public string? ReviewedBy { get; set; }
}