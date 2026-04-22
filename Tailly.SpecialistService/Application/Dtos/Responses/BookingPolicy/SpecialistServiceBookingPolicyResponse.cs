namespace Tailly.SpecialistService.Application.Dtos.Responses.BookingPolicy;

public sealed class SpecialistServiceBookingPolicyResponse
{
    public string Mode { get; set; } = "fixed_slot";                    
    public SpecialistServiceDurationPolicyResponse Duration { get; set; } = new();
    public SpecialistServiceBufferPolicyResponse Buffer { get; set; } = new();
    public SpecialistServiceCompatibilityPolicyResponse Compatibility { get; set; } = new();
    public SpecialistServiceAdvancePolicyResponse Advance { get; set; } = new();
    public SpecialistServiceMultiDayPolicyResponse? MultiDay { get; set; }
    public bool AllowsClientComment { get; set; } = true;
    public bool RequiresSpecialistConfirmation { get; set; } = false;
}