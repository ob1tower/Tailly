namespace Tailly.AuthService.Application.Dtos.Requests;

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}