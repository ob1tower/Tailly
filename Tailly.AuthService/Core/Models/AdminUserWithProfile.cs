using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Core.Models;

public class AdminUserWithProfile
{
    public UserEntity User { get; set; } = null!;
    public UserRoleEntity Role { get; set; } = null!;
    public AdminProfileEntity? Profile { get; set; }
}