namespace Tailly.SpecialistService.Application.Dtos.Requests.Media;

public sealed class UploadMediaRequest
{
    public IFormFile File { get; set; } = default!;
    public string MediaType { get; set; } = default!;
}
