namespace Tailly.BookingService.Application.Dtos.Responses;

public sealed class ClientServiceOrderResponse
{
    public Guid Id { get; set; }
    public string Number { get; set; } = default!;
    public Guid SpecialistId { get; set; }
    public string SpecialistName { get; set; } = default!;
    public string SpecialistSlug { get; set; } = default!;
    public Guid PetId { get; set; }
    public string PetName { get; set; } = default!;
    public Guid ServiceId { get; set; }
    public string ServiceTitle { get; set; } = default!;
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string? CancelReason { get; set; }
    public bool HasReview { get; set; }
    public string Currency { get; set; } = "RUB";
}