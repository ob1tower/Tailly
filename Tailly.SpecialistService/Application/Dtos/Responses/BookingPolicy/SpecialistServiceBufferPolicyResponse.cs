namespace Tailly.SpecialistService.Application.Dtos.Responses.BookingPolicy;

public sealed class SpecialistServiceBufferPolicyResponse
{
    public bool HasBufferBefore { get; set; } = false;
    public int BufferBeforeMinutes { get; set; } = 0;
    public bool HasBufferAfter { get; set; } = false;
    public int BufferAfterMinutes { get; set; } = 0;
}