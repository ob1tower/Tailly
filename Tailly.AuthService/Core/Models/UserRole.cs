using Tailly.AuthService.Core.Enums;

namespace Tailly.AuthService.Core.Models;

public class UserRole
{
    public RoleType Role { get; set; }
    public DateTime? SoftDeletedAt { get; set; }
    public DateTime? RestoreUntil { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsPermanentBlock { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public string? BlockReason { get; set; }

    public bool IsEffectivelyBlocked =>
            IsPermanentBlock
            || (BlockedUntil.HasValue && BlockedUntil.Value > DateTime.UtcNow)
            || (IsBlocked && !BlockedUntil.HasValue);

    public bool IsPendingDeletion =>
            SoftDeletedAt.HasValue
            && RestoreUntil.HasValue
            && RestoreUntil.Value > DateTime.UtcNow;
}