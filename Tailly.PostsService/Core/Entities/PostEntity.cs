namespace Tailly.PostsService.Core.Entities;

public class PostEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public ICollection<PostImageEntity> Images { get; set; } = [];
    public ICollection<PostTagEntity> Tags { get; set; } = [];
}