namespace Tailly.AuthService.Application.Dtos.Requests.SuperAdmin;

public sealed class UpdateAdminBlockStatusPayload
{
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public bool? IsPermanentBlock { get; set; }
}