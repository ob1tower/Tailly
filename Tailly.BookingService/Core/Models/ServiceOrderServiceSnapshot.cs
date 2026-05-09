using Tailly.BookingService.Core.Enums;

namespace Tailly.BookingService.Core.Models;

public class ServiceOrderServiceSnapshot
{
    public Guid ServiceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PriceUnit PriceUnit { get; set; }
}