using Tailly.AuthService.Application.Dtos.Common;

namespace Tailly.AuthService.Application.Dtos.Responses;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public DateTime AccessTokenExpires { get; set; }
    public string RefreshToken { get; set; } = default!;
    public DateTime RefreshTokenExpires { get; set; }
    public AuthUserDto User { get; set; } = null!;
}