namespace Tailly.SpecialistService.Application.Dtos.Responses.BookingPolicy;

public sealed class SpecialistServiceAdvancePolicyResponse
{
    public int? MinAdvanceMinutes { get; set; }
    public int? MaxAdvanceDays { get; set; }
}