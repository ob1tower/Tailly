namespace Tailly.BookingService.Core.Entities;

public class ServiceOrderReviewEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public ServiceOrderEntity Order { get; set; } = null!;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Photos { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}