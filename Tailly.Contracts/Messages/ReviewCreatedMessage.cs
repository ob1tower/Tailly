namespace Tailly.Contracts.Messages;

public sealed class ReviewCreatedMessage
{
    public Guid ReviewId { get; init; }
    public Guid SpecialistId { get; init; }
    public Guid OrderId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string ServiceTitle { get; init; } = string.Empty;
    public string PetName { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<string> Photos { get; set; } = [];
}