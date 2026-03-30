namespace Tailly.AuthService.Core.Models;

public class EmailChangeResult
{
    public string RequestId { get; set; } = string.Empty;
    public string MaskedOldEmail { get; set; } = string.Empty;
}