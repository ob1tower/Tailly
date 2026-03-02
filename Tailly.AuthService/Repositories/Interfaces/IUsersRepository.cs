using Tailly.AuthService.Models;

namespace Tailly.AuthService.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task AddAsync(User user);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
    }
}