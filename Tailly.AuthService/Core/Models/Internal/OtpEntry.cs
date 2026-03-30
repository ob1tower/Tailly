namespace Tailly.AuthService.Core.Models.Internal;

public class OtpEntry
{
    public string CodeHash { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public string Purpose { get; set; } = "default";
    public DateTime CreatedAt { get; set; }
}