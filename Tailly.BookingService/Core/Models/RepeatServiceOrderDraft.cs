using Tailly.BookingService.Core.Enums;

namespace Tailly.BookingService.Core.Models;

public class RepeatServiceOrderDraft
{
    public Guid PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public Guid SpecialistId { get; set; }
    public string SpecialistName { get; set; } = string.Empty;
    public string SpecialistSlug { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PriceUnit PriceUnit { get; set; }
    public string? Comment { get; set; }
}