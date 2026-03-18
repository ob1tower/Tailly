namespace Tailly.AuthService.Entities;

public class UserRoleEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = default!;

    public int RoleId { get; set; }
    public RoleEntity Role { get; set; } = default!;
}
