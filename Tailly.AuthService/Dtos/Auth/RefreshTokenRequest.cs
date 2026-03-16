namespace Tailly.AuthService.Dtos.Auth;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}