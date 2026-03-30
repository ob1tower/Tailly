namespace Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;

public sealed class ResetPasswordPayload
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}