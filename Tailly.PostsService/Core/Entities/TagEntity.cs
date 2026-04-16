namespace Tailly.PostsService.Core.Entities;

public class TagEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<PostTagEntity> PostTags { get; set; } = [];
}