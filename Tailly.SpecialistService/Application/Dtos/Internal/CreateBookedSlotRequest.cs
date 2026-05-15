namespace Tailly.SpecialistService.Application.Dtos.Internal;

public sealed class CreateBookedSlotRequest
{
    public Guid OrderId { get; set; }
    public Guid SpecialistId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}