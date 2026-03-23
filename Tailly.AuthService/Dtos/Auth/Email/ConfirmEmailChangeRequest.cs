namespace Tailly.AuthService.Dtos.Auth.Email;

public sealed class ConfirmEmailChangeRequest
{
    public string NewEmail { get; set; } = default!;
    public string Code { get; set; } = default!;
}
