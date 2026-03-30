using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Infrastructure.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task InvalidateAsync(string token);
    Task<int> RemoveExpiredTokensAsync();
    Task InvalidateAllAsync(Guid userId);
}