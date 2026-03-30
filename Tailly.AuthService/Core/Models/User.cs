using Tailly.AuthService.Core.Enums;

namespace Tailly.AuthService.Core.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool EmailConfirmed { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsPermanentBlock { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public DateTime? SoftDeletedAt { get; set; }
    public DateTime? RestoreUntil { get; set; }
    public string? SpecialistSlug { get; set; }

    public Guid? SpecialistId { get; set; } = null;
    public Guid? AdminId { get; set; } = null;

    public List<RoleType> Roles { get; set; } = [];
}