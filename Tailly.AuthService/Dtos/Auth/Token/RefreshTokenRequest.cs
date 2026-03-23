namespace Tailly.AuthService.Dtos.Auth.Token;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}