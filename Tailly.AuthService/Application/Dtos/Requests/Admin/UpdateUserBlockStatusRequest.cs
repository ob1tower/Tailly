namespace Tailly.AuthService.Application.Dtos.Requests.Admin;

public sealed class UpdateUserBlockStatusRequest
{
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public bool? IsPermanentBlock { get; set; }
}