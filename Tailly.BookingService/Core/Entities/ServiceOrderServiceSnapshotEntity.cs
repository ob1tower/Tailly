using Tailly.BookingService.Core.Enums;

namespace Tailly.BookingService.Core.Entities;

public class ServiceOrderServiceSnapshotEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public ServiceOrderEntity Order { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PriceUnit PriceUnit { get; set; }
}