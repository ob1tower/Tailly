using Tailly.AuthService.Application.Dtos.Common;

namespace Tailly.AuthService.Application.Dtos.Responses.Register;

public sealed class RegisterCompleteResponse
{
    public string AccessToken { get; set; } = default!;
    public AuthUserDto User { get; set; } = default!;
}