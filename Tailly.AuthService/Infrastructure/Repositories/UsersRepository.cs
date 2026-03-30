using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.DataAccess;
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
            IsBlocked = user.IsBlocked,
            IsPermanentBlock = user.IsPermanentBlock,
            BlockedUntil = user.BlockedUntil,
            SoftDeletedAt = user.SoftDeletedAt,
            RestoreUntil = user.RestoreUntil,
            SpecialistSlug = user.SpecialistSlug,
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

        return new User
        {
            Id = userEntity.Id,
            Email = userEntity.Email,
            PasswordHash = userEntity.PasswordHash,
            CreatedAt = userEntity.CreatedAt,
            EmailConfirmed = userEntity.EmailConfirmed,

            Roles = userEntity.UserRoles
                .Select(x => (RoleType)x.RoleId)
                .ToList(),

            IsBlocked = userEntity.IsBlocked,
            IsPermanentBlock = userEntity.IsPermanentBlock,
            BlockedUntil = userEntity.BlockedUntil,
            SoftDeletedAt = userEntity.SoftDeletedAt,
            RestoreUntil = userEntity.RestoreUntil,
            SpecialistSlug = userEntity.SpecialistSlug,
            SpecialistId = userEntity.SpecialistId,
            AdminId = userEntity.AdminId
        };
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

        return new User
        {
            Id = userEntity.Id,
            Email = userEntity.Email,
            PasswordHash = userEntity.PasswordHash,
            CreatedAt = userEntity.CreatedAt,
            EmailConfirmed = userEntity.EmailConfirmed,

            Roles = userEntity.UserRoles
                .Select(x => (RoleType)x.RoleId)
                .ToList(),

            IsBlocked = userEntity.IsBlocked,
            IsPermanentBlock = userEntity.IsPermanentBlock,
            BlockedUntil = userEntity.BlockedUntil,
            SoftDeletedAt = userEntity.SoftDeletedAt,
            RestoreUntil = userEntity.RestoreUntil,
            SpecialistSlug = userEntity.SpecialistSlug,
            SpecialistId = userEntity.SpecialistId,
            AdminId = userEntity.AdminId
        };
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
        userEntity.IsBlocked = user.IsBlocked;
        userEntity.IsPermanentBlock = user.IsPermanentBlock;
        userEntity.BlockedUntil = user.BlockedUntil;
        userEntity.SoftDeletedAt = user.SoftDeletedAt;
        userEntity.RestoreUntil = user.RestoreUntil;
        userEntity.SpecialistSlug = user.SpecialistSlug;
        userEntity.SpecialistId = user.SpecialistId;
        userEntity.AdminId = user.AdminId;

        await _authDbContext.SaveChangesAsync();
    }
}