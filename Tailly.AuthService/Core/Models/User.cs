using Tailly.AuthService.Core.Enums;

namespace Tailly.AuthService.Core.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool EmailConfirmed { get; set; }
    public string? SpecialistSlug { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public Guid? SpecialistId { get; set; } = null;
    public Guid? AdminId { get; set; } = null;
    public List<RoleType> Roles { get; set; } = [];
    public List<UserRole> UserRoles { get; set; } = [];

    public UserRole? GetRole(RoleType role) =>
        UserRoles.FirstOrDefault(x => x.Role == role);
}