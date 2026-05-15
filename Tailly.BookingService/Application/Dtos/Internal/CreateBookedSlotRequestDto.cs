namespace Tailly.BookingService.Application.Dtos.Internal;

public sealed class CreateBookedSlotRequestDto
{
    public Guid SpecialistId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}