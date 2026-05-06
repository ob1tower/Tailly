using Tailly.AuthService.Core.Enums;

namespace Tailly.AuthService.Core.Entities;

public class AdminProfileEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Phone { get; set; }
    public AdminDepartment? Department { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? PasswordAttemptsLockUntil { get; set; }
    public int FailedPasswordAttempts { get; set; } = 0;
}