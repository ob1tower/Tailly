namespace Tailly.AuthService.Dtos.Auth;

public sealed class ConfirmEmailRequest
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
}
