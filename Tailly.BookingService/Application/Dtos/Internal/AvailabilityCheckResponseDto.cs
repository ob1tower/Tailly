namespace Tailly.BookingService.Application.Dtos.Internal;

public sealed class AvailabilityCheckResponseDto
{
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
}