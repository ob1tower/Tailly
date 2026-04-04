namespace Tailly.Contracts.Messages;

public record UserRegisteredMessage
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public string? Phone { get; init; } = string.Empty;
    public string? CityName { get; init; } = string.Empty;
    public string CityId { get; init; } = string.Empty;
}