namespace Tailly.AuthService.Dtos.Auth;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } =default!;
}
