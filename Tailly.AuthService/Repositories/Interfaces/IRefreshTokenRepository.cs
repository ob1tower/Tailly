using Tailly.AuthService.Models;

namespace Tailly.AuthService.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task InvalidateAsync(string token);
        Task<int> RemoveExpiredTokensAsync();
        Task InvalidateAllAsync(Guid userId);
    }
}