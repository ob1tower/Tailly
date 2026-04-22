using Tailly.SpecialistService.Core.Entities.Specialist;

namespace Tailly.SpecialistService.Core.Entities.Reviews;

public class ReviewEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = default!;
    public string AuthorName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ServiceTitle { get; set; }
    public string? PetName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReplyCreatedAt { get; set; }
    public string? ReplyText { get; set; }
}