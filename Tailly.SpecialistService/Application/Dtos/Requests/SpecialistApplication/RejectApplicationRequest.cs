namespace Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

public sealed class RejectApplicationRequest
{
    public string Reason { get; set; } = default!;
}