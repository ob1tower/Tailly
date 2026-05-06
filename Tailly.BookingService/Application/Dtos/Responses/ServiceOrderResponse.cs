namespace Tailly.BookingService.Application.Dtos.Responses;

public sealed class ServiceOrderResponse
{
    public Guid Id { get; set; }
    public string Number { get; set; } = default!;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = default!;
    public Guid SpecialistId { get; set; }
    public string SpecialistName { get; set; } = default!;
    public string SpecialistSlug { get; set; } = default!;
    public Guid PetId { get; set; }
    public string PetName { get; set; } = default!;
    public string ServiceId { get; set; } = default!;
    public string ServiceTitle { get; set; } = default!;
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string? CancelReason { get; set; }
}