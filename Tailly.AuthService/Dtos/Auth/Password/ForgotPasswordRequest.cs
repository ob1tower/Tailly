namespace Tailly.AuthService.Dtos.Auth.Password;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } =default!;
}
