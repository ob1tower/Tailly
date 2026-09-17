using Tailly.AuthService.Core.Enums;

namespace Tailly.AuthService.Core.Entities;

public class AdminPasswordRecoveryEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public AdminPasswordRecoveryStatus Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? TemporaryPassword { get; set; }
    public Guid? ProcessedBy { get; set; } 
}