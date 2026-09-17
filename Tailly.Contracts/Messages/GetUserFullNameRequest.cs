namespace Tailly.Contracts.Messages;

public record GetUserFullNameRequest
{
    public Guid UserId { get; init; }
}