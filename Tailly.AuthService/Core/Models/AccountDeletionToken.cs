namespace Tailly.AuthService.Core.Models;

public class AccountDeletionToken
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}