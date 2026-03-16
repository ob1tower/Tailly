using Tailly.AuthService.Models;

namespace Tailly.AuthService.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task AddAsync(User user);
        Task<bool> ExistsAsync(string email, int roleId);
        Task<User?> GetByEmailAndRoleAsync(string email, int roleId);
        Task<User?> GetByIdAsync(Guid id);
    }
}