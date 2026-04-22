using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Infrastructure.Repositories.Interfaces
{
    public interface IAccountDeletionTokenRepository
    {
        Task AddAsync(AccountDeletionToken model);
        Task<AccountDeletionToken?> GetAsync(string token);
        Task RemoveAsync(string token);
        Task RemoveExpiredAsync();
    }
}