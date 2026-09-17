using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Infrastructure.Repositories.Interfaces;

public interface IUsersRepository
{
    Task AddAsync(User user);
    Task<bool> ExistsAsync(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddRoleAsync(Guid userId, int roleId);
    Task DeleteUserAsync(Guid userId);
    Task DeleteUserRolesAsync(Guid userId);
    Task UpdateAsync(User user);
    IQueryable<UserEntity> Query();
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetAdminsAsync(int page, int pageSize);
    Task<int> CountAdminsAsync();
    Task<User?> GetAdminByAdminIdAsync(Guid adminId);
    Task<int> PatchUserSoftDeleteAsync(Guid userId, DateTime softDeletedAt, DateTime? restoreUntil);
    Task<int> RemoveExpiredDeletedRolesAsync();
    Task<List<AdminUserWithProfile>> GetAdminsWithProfilesAsync(int page, int pageSize);
    Task<int> PatchUserRoleSoftDeleteAsync(Guid userId, RoleType role, DateTime? softDeletedAt, DateTime? restoreUntil);
    Task<int> PatchUserRoleBlockAsync(Guid userId, RoleType role, bool isBlocked, bool isPermanentBlock, DateTime? blockedUntil, string? blockReason);
}