using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.DataAccess;
using Tailly.AuthService.Entities;
using Tailly.AuthService.Enums;
using Tailly.AuthService.Models;
using Tailly.AuthService.Repositories.Interfaces;

namespace Tailly.AuthService.Repositories;

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
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            CreatedAt = user.CreatedAt
        };

        await _authDbContext.Users.AddAsync(userEntity);
        await _authDbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var userEntity = await _authDbContext.Users
            .Include(x => x.UserRoles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

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
                .ToList()
        };
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _authDbContext.Users
            .AnyAsync(u => u.Email == email);
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
                .ToList()
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

        userEntity.PasswordHash = user.PasswordHash;
        userEntity.EmailConfirmed = user.EmailConfirmed;

        await _authDbContext.SaveChangesAsync();
    }
}