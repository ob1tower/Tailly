namespace Tailly.AuthService.Application.Dtos.Requests.AccountDeletion;

public sealed class RestoreAccountByTokenRequest
{
    public string Token { get; set; } = default!;
}