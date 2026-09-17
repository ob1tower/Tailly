namespace Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;

public sealed class VerifyRecoveryCodePayload
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
}