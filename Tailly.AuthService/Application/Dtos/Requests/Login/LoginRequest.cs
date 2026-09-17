namespace Tailly.AuthService.Application.Dtos.Requests.Login;

public sealed class LoginRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string RequestedRole { get; set; } = default!;
}