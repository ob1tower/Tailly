namespace Tailly.AuthService.Dtos.Auth;

public sealed class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public int RoleId { get; set; }
}