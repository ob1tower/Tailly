namespace Tailly.BookingService.Application.Dtos.Requests;

public sealed class LeaveReviewRequest
{
    public int Rating { get; set; }
    public string Comment { get; set; } = default!;
    public List<string> Photos { get; set; } = [];
}