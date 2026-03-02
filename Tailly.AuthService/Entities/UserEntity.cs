namespace Tailly.AuthService.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int RoleId { get; set; }
    public RoleEntity Role { get; set; } = default!;
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
}
