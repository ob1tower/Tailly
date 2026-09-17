namespace Tailly.BookingService.Application.Dtos.Requests;

public sealed class CreateServiceOrderRequest
{
    public Guid SpecialistId { get; set; }
    public Guid PetId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? Comment { get; set; }
}