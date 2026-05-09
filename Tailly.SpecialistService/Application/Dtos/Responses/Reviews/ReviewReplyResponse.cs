namespace Tailly.SpecialistService.Application.Dtos.Responses.Reviews;

public sealed class ReviewReplyResponse
{
    public string Text { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}