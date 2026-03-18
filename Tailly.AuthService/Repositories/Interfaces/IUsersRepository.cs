using Tailly.AuthService.Models;

namespace Tailly.AuthService.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task AddAsync(User user);
        Task<bool> ExistsAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task AddRoleAsync(Guid userId, int roleId);
    }
}