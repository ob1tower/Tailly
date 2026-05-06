namespace Tailly.AuthService.Core.Models;

public class AdminProfile
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string Role { get; set; } = "admin";
}