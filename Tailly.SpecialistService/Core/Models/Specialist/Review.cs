namespace Tailly.SpecialistService.Core.Models.Specialist;

public class Review
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ServiceTitle { get; set; }
    public string? PetName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReplyText { get; set; }
    public DateTime? ReplyCreatedAt { get; set; }
}