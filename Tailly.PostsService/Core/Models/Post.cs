using Tailly.PostsService.Core.Enums;

namespace Tailly.PostsService.Core.Models;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public PostStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public Guid CreatedBy { get; set; }
}