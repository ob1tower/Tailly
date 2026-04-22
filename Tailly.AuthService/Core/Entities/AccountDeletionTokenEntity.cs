namespace Tailly.AuthService.Core.Entities;

public class AccountDeletionTokenEntity
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserEntity User { get; set; } = default!;
}