namespace Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;

public sealed class SendRecoveryCodePayload
{
    public string Email { get; set; } = default!;
}