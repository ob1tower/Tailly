namespace Tailly.PostsService.Application.Dtos.Responses;

public sealed class PostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? CoverImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
