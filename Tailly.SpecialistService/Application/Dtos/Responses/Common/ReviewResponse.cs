namespace Tailly.SpecialistService.Application.Dtos.Responses.Common;

public sealed class ReviewResponse
{
    public string AuthorName { get; set; } = default!;
    public int Rating { get; set; }
    public string Text { get; set; } = default!;
    public string? ServiceTitle { get; set; }
    public string? PetName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReplyText { get; set; }
    public DateTime? ReplyCreatedAt { get; set; }
}