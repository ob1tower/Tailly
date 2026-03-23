namespace Tailly.AuthService.Dtos.Auth.Email;

public sealed class ChangeEmailRequest
{
    public string NewEmail { get; set; } = default!;
}
