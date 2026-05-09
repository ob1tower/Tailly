namespace Tailly.BookingService.Core.Models;

public class CreateServiceOrder
{
    public Guid ClientId { get; set; }
    public Guid SpecialistId { get; set; }
    public Guid PetId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? Comment { get; set; }
    public ServiceOrderServiceSnapshot? ServiceSnapshot { get; set; }
}