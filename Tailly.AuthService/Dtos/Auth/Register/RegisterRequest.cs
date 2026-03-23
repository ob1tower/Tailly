namespace Tailly.AuthService.Dtos.Auth.Register;

public sealed class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}