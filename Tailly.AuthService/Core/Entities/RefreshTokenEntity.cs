namespace Tailly.AuthService.Core.Entities;

public class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }

    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = default!;
}