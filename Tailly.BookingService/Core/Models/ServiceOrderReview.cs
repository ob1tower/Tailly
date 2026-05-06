namespace Tailly.BookingService.Core.Models;

public class ServiceOrderReview
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Photos { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}