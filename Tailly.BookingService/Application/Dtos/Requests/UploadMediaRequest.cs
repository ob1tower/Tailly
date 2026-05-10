namespace Tailly.BookingService.Application.Dtos.Requests;

public sealed class UploadMediaRequest
{
    public IFormFile File { get; set; } = default!;
    public string MediaType { get; set; } = default!;
}