using Tailly.AuthService.Core.Enums;

namespace Tailly.AuthService.Core.Models;

public class AdminPasswordRecovery
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public AdminPasswordRecoveryStatus Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? TemporaryPassword { get; set; }
    public Guid? ProcessedBy { get; set; }
}