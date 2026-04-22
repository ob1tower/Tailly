namespace Tailly.SpecialistService.Application.Dtos.Responses.BookingPolicy;

public sealed class SpecialistServiceMultiDayPolicyResponse
{
    public bool AllowsMultiDayBooking { get; set; } = false;
    public int? MinStayDays { get; set; }
    public int? MaxStayDays { get; set; }
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
}