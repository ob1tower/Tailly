namespace Tailly.AuthService.Dtos.Auth;

public sealed class LogoutRequest
{
    public string RefreshToken { get; set; } = default!;
}