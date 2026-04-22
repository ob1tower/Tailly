namespace Tailly.AuthService.Core.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool EmailConfirmed { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsPermanentBlock { get; set; }
    public string? BlockReason { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public DateTime? SoftDeletedAt { get; set; }
    public DateTime? RestoreUntil { get; set; }
    public string? SpecialistSlug { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
    public Guid? SpecialistId { get; set; }
    public Guid? AdminId { get; set; }
}