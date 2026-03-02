namespace Tailly.AuthService.Entities;

public class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = default!;
}
