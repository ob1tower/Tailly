namespace Tailly.PostsService.Application.Dtos.Responses;

public sealed class BannerResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string LinkTarget { get; set; } = default!;
    public string Placement { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}