using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.DataAccess;
using Tailly.AuthService.Entities;
using Tailly.AuthService.Models;
using Tailly.AuthService.Repositories.Interfaces;

namespace Tailly.AuthService.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _authDbContext;

    public RefreshTokenRepository(AuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }

    public async Task AddAsync(RefreshToken token)
    {
        var tokenEntity = new RefreshTokenEntity
        {
            Id = token.Id,
            Token = token.Token,
            CreatedAt = token.CreatedAt,
            ExpiresAt = token.ExpiresAt,
            RevokedAt = token.RevokedAt,
            UserId = token.UserId
        };

        await _authDbContext.RefreshTokens.AddAsync(tokenEntity);
        await _authDbContext.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        var tokenEntity = await _authDbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == token);

        if (tokenEntity == null)
            return null;

        return new RefreshToken
        {
            Id = tokenEntity.Id,
            Token = tokenEntity.Token,
            CreatedAt = tokenEntity.CreatedAt,
            ExpiresAt = tokenEntity.ExpiresAt,
            RevokedAt = tokenEntity.RevokedAt,
            UserId = tokenEntity.UserId
        };
    }

    public async Task InvalidateAsync(string token)
    {
        var tokenEntity = await _authDbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token);

        if (tokenEntity == null)
            return;

        tokenEntity.RevokedAt = DateTime.UtcNow;

        await _authDbContext.SaveChangesAsync();
    }
}
