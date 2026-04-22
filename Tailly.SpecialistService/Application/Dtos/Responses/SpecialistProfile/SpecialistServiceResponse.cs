using Tailly.SpecialistService.Application.Dtos.Responses.BookingPolicy;

namespace Tailly.SpecialistService.Application.Dtos.Responses.SpecialistProfile;

public sealed class SpecialistServiceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = default!;
    public string LocationLabel { get; set; } = default!;
    public string ServiceId { get; set; } = default!;
    public SpecialistServiceBookingPolicyResponse? BookingPolicy { get; set; }
}