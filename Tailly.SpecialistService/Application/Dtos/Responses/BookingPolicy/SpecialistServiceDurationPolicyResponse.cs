namespace Tailly.SpecialistService.Application.Dtos.Responses.BookingPolicy;

public sealed class SpecialistServiceDurationPolicyResponse
{
    public int? DefaultDurationMinutes { get; set; }
    public int? MinDurationMinutes { get; set; }
    public int? MaxDurationMinutes { get; set; }
    public int? DurationStepMinutes { get; set; } = 30;
}