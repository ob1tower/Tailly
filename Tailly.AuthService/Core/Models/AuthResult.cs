using Tailly.AuthService.Application.Dtos.Common;

namespace Tailly.AuthService.Core.Models;

public class AuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpires { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
    public AuthUserDto User { get; set; } = null!;
}