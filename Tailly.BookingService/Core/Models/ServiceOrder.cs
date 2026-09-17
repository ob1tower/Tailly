using Tailly.BookingService.Core.Enums;

namespace Tailly.BookingService.Core.Models;

public class ServiceOrder
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid SpecialistId { get; set; }
    public string SpecialistName { get; set; } = string.Empty;
    public string SpecialistSlug { get; set; } = string.Empty;
    public Guid PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PriceUnit PriceUnit { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? Comment { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string? CancelReason { get; set; }
    public ServiceOrderReview? Review { get; set; }
    public ServiceOrderServiceSnapshot? ServiceSnapshot { get; set; }
}