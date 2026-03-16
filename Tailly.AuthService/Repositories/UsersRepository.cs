using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.DataAccess;
using Tailly.AuthService.Entities;
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
        var userEntity = new UserEntity()
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            CreatedAt = DateTime.UtcNow,
            RoleId = user.RoleId
        };
        await _authDbContext.Users.AddAsync(userEntity);
        await _authDbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAndRoleAsync(string email, int roleId)
    {
        var userEntity = await _authDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.RoleId == roleId);

        if (userEntity == null)
            return null;

        return new User
        {
            Id = userEntity.Id,
            Email = userEntity.Email,
            PasswordHash = userEntity.PasswordHash,
            CreatedAt = userEntity.CreatedAt,
            RoleId = userEntity.RoleId
        };
    }

    public async Task<bool> ExistsAsync(string email, int roleId)
    {
        return await _authDbContext.Users
            .AnyAsync(u => u.Email == email && u.RoleId == roleId);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var userEntity = await _authDbContext.Users
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
            RoleId = userEntity.RoleId
        };
    }
}