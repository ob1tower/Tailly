namespace Tailly.AuthService.Infrastructure.Messaging.Messages;

public record SendEmailMessage
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? Purpose { get; init; }
}