namespace Tailly.AuthService.Core.Entities;

public class RoleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];
}