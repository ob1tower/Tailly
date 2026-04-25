namespace Tailly.PostsService.Application.Dtos.Requests;

public sealed class SaveBannerRequest
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}