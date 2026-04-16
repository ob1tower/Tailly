namespace Tailly.PostsService.Application.Dtos.Requests;

public sealed class CreatePostRequest
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? CoverImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string Status { get; set; } = default!;
}