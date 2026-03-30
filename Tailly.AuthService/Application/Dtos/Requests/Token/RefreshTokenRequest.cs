namespace Tailly.AuthService.Application.Dtos.Requests.Token;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}