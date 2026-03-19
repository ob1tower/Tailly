namespace Tailly.AuthService.Dtos.Auth;

public sealed class ResetPasswordRequest
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
