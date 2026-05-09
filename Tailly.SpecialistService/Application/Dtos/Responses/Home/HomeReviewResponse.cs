namespace Tailly.SpecialistService.Application.Dtos.Responses.Home;

public sealed class HomeReviewResponse
{
    public Guid Id { get; set; }
    public string CreatedAtIso { get; set; } = default!;
    public int Rating { get; set; }
    public string Text { get; set; } = default!;
    public string? PetName { get; set; }
    public string OwnerName { get; set; } = default!;
    public Guid SitterId { get; set; }
    public string SitterName { get; set; } = default!;
    public string? ServiceTitle { get; set; }
    public List<string> PhotoUrls { get; set; } = [];
}