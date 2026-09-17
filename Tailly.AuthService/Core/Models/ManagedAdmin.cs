namespace Tailly.AuthService.Core.Models;

public class ManagedAdmin
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string Role { get; set; } = "admin";
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public bool IsPermanentBlock { get; set; }
    public DateTime? PasswordAttemptsLockUntil { get; set; }
    public int FailedPasswordAttempts { get; set; } = 0;
}