namespace Tailly.AuthService.Core.Models.Internal;

public class RegistrationData
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}