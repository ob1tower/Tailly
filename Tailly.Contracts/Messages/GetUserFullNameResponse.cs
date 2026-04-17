namespace Tailly.Contracts.Messages;

public record GetUserFullNameResponse
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}