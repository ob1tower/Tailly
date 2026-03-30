namespace Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;

public sealed class StartPasswordRecoveryPayload
{
    public string Email { get; set; } = default!;
}