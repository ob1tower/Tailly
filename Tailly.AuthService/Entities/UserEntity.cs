namespace Tailly.AuthService.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];

    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
}
