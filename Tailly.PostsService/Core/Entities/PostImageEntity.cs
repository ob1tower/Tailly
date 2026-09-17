namespace Tailly.PostsService.Core.Entities;

public class PostImageEntity
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public PostEntity? Post { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsCover { get; set; }
}