namespace Tailly.AuthService.Core.Entities;

public class UserRoleEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = default!;
    public int RoleId { get; set; }
    public RoleEntity Role { get; set; } = default!;
    public DateTime? SoftDeletedAt { get; set; }
    public DateTime? RestoreUntil { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsPermanentBlock { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public string? BlockReason { get; set; }

    public bool IsEffectivelyBlocked =>
        IsPermanentBlock ||
        (BlockedUntil.HasValue && BlockedUntil.Value > DateTime.UtcNow) ||
        (IsBlocked && !BlockedUntil.HasValue);
}