namespace Tailly.AuthService.Application.Dtos.Requests.AccountDeletion;

public sealed class AccountDeletionRequest
{
    public string UserId { get; set; } = default!;
    public string Password { get; set; } = default!;
}