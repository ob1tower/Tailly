using Tailly.AuthService.Application.Dtos.Common;

namespace Tailly.AuthService.Application.Dtos.Responses;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = default!;
    public AuthUserDto User { get; set; } = default!;
}