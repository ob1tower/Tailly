namespace Tailly.BookingService.Application.Dtos.Requests;

public class UploadMediaRequest
{
    public IFormFile File { get; set; } = default!;
    public string MediaType { get; set; } = default!;
}