namespace Tailly.SpecialistService.Application.Dtos.Responses.Reviews;

public sealed class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public string AuthorName { get; set; } = default!;
    public string Text { get; set; } = default!;
    public int Rating { get; set; }
    public string ServiceTitle { get; set; } = default!;
    public string PetName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string ReplyText { get; set; } = default!;
    public DateTime? ReplyCreatedAt { get; set; }
}