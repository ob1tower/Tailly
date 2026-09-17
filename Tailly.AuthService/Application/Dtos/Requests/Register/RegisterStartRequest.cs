namespace Tailly.AuthService.Application.Dtos.Requests.Register;

public sealed class RegisterStartRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}