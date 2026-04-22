namespace Tailly.Contracts.Messages;

public record UserProfileUpdatedMessage
{
    public Guid UserId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? MiddleName { get; init; }
    public string? SpecialistSlug { get; init; }
}