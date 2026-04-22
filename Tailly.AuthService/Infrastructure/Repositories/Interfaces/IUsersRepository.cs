using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Infrastructure.Repositories.Interfaces;

public interface IUsersRepository
{
    Task AddAsync(User user);
    Task<bool> ExistsAsync(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddRoleAsync(Guid userId, int roleId);
    Task UpdateAsync(User user);
    IQueryable<UserEntity> Query();
    Task<List<User>> GetAllAsync();
}