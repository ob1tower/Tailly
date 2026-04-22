namespace Tailly.SpecialistService.Application.Dtos.Responses.BookingPolicy;

public sealed class SpecialistServiceCompatibilityPolicyResponse
{
    public bool CanOverlapWithOtherServices { get; set; } = false;
    public List<string> CompatibleServiceIds { get; set; } = [];
}