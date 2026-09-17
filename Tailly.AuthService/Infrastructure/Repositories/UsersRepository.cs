using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.DataAccess;
using Tailly.AuthService.Infrastructure.Mappers;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Infrastructure.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly AuthDbContext _authDbContext;

    public UsersRepository(AuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }

    public async Task AddAsync(User user)
    {
        var userEntity = new UserEntity
        {
            Id = user.Id,
            Email = user.Email.ToLower(),
            PasswordHash = user.PasswordHash,
            CreatedAt = user.CreatedAt,
            EmailConfirmed = user.EmailConfirmed,
            SpecialistSlug = user.SpecialistSlug,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            SpecialistId = user.SpecialistId,
            AdminId = user.AdminId
        };

        await _authDbContext.Users.AddAsync(userEntity);
        await _authDbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var userEntity = await _authDbContext.Users
            .Include(x => x.UserRoles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

        if (userEntity == null)
            return null;

        return UserEntityMapper.ToDomain(userEntity);
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _authDbContext.Users
            .AnyAsync(u => u.Email == email.ToLower());
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var userEntity = await _authDbContext.Users
            .Include(x => x.UserRoles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (userEntity == null)
            return null;

        return UserEntityMapper.ToDomain(userEntity);
    }

    public async Task AddRoleAsync(Guid userId, int roleId)
    {
        var exists = await _authDbContext.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId);

        if (exists)
            return;

        var userRole = new UserRoleEntity
        {
            UserId = userId,
            RoleId = roleId
        };

        await _authDbContext.UserRoles.AddAsync(userRole);
        await _authDbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        var userEntity = await _authDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == user.Id);

        if (userEntity == null)
            return;

        userEntity.Email = user.Email;
        userEntity.PasswordHash = user.PasswordHash;
        userEntity.EmailConfirmed = user.EmailConfirmed;
        userEntity.SpecialistSlug = user.SpecialistSlug;
        userEntity.FirstName = user.FirstName;
        userEntity.LastName = user.LastName;
        userEntity.MiddleName = user.MiddleName;
        userEntity.SpecialistId = user.SpecialistId;
        userEntity.AdminId = user.AdminId;

        await _authDbContext.SaveChangesAsync();
    }

    public async Task<List<User>> GetAllAsync()
    {
        var userEntities = await _authDbContext.Users
            .Include(x => x.UserRoles)
            .AsNoTracking()
            .ToListAsync();

        if (!userEntities.Any())
            return [];

        return userEntities.Select(UserEntityMapper.ToDomain).ToList();
    }

    public IQueryable<UserEntity> Query()
    {
        return _authDbContext.Users
            .Include(x => x.UserRoles)
            .AsNoTracking();
    }

    public async Task<int> PatchUserRoleBlockAsync(Guid userId, RoleType role, bool isBlocked, bool isPermanentBlock, DateTime? blockedUntil, string? blockReason)
    {
        return await _authDbContext.UserRoles
            .Where(x => x.UserId == userId && x.RoleId == (int)role)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ur => ur.IsBlocked, isBlocked)
                .SetProperty(ur => ur.IsPermanentBlock, isPermanentBlock)
                .SetProperty(ur => ur.BlockedUntil, blockedUntil)
                .SetProperty(ur => ur.BlockReason, blockReason));
    }

    public async Task<int> PatchUserRoleSoftDeleteAsync(Guid userId, RoleType role, DateTime? softDeletedAt, DateTime? restoreUntil)
    {
        return await _authDbContext.UserRoles
            .Where(x => x.UserId == userId && x.RoleId == (int)role)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ur => ur.SoftDeletedAt, softDeletedAt)
                .SetProperty(ur => ur.RestoreUntil, restoreUntil));
    }

    public async Task<int> RemoveExpiredDeletedRolesAsync()
    {
        var now = DateTime.UtcNow;

        var deletedCount = await _authDbContext.UserRoles
            .Where(ur => ur.SoftDeletedAt.HasValue &&
                         ur.RestoreUntil.HasValue &&
                         ur.RestoreUntil.Value < now)
            .ExecuteDeleteAsync();

        return deletedCount;
    }

    public async Task<List<User>> GetAdminsAsync(int page, int pageSize)
    {
        var users = await _authDbContext.Users
            .Include(x => x.UserRoles)
            .Where(u => u.UserRoles.Any(r =>
                r.RoleId == (int)RoleType.Admin &&
                r.SoftDeletedAt == null))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return users.Select(UserEntityMapper.ToDomain).ToList();
    }

    public async Task<int> CountAdminsAsync()
    {
        return await _authDbContext.Users
            .CountAsync(u => u.UserRoles.Any(r =>
                r.RoleId == (int)RoleType.Admin &&
                r.SoftDeletedAt == null));
    }

    public async Task<User?> GetAdminByAdminIdAsync(Guid adminId)
    {
        var userEntity = await _authDbContext.Users
            .Include(x => x.UserRoles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.AdminId == adminId &&
                u.UserRoles.Any(r =>
                    (r.RoleId == (int)RoleType.Admin ||
                     r.RoleId == (int)RoleType.SuperAdmin)));

        if (userEntity == null)
            return null;

        return UserEntityMapper.ToDomain(userEntity);
    }

    public async Task<List<AdminUserWithProfile>> GetAdminsWithProfilesAsync(int page, int pageSize)
    {
        var query = _authDbContext.Users
            .Include(u => u.UserRoles)
            .Include(u => u.AdminProfile)
            .Where(u => u.UserRoles.Any(r =>
                r.RoleId == (int)RoleType.Admin &&          
                r.SoftDeletedAt == null))
            .SelectMany(u => u.UserRoles, (user, role) => new AdminUserWithProfile
            {
                User = user,
                Role = role,
                Profile = user.AdminProfile
            })
            .Where(x => x.Role.RoleId == (int)RoleType.Admin)
            .OrderBy(x => x.User.LastName)
            .ThenBy(x => x.User.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync();
    }

    public async Task<int> PatchUserSoftDeleteAsync(Guid userId, DateTime softDeletedAt, DateTime? restoreUntil)
    {
        return await _authDbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.SoftDeletedAt, softDeletedAt)
                .SetProperty(u => u.RestoreUntil, restoreUntil));
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        await _authDbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteDeleteAsync();
    }

    public async Task DeleteUserRolesAsync(Guid userId)
    {
        await _authDbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .ExecuteDeleteAsync();
    }
}