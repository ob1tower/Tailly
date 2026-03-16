namespace Tailly.AuthService.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }
    public Guid UserId { get; set; }

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => Revoked == null && !IsExpired;
}
