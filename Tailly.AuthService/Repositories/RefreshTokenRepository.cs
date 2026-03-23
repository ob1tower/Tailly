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
            TokenHash = token.TokenHash,
            Created = token.Created,
            Expires = token.Expires,
            Revoked = token.Revoked,
            UserId = token.UserId
        };

        await _authDbContext.RefreshTokens.AddAsync(tokenEntity);
        await _authDbContext.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        var tokenEntity = await _authDbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == token);

        if (tokenEntity == null)
            return null;

        return new RefreshToken
        {
            Id = tokenEntity.Id,
            TokenHash = tokenEntity.TokenHash,
            Created = tokenEntity.Created,
            Expires = tokenEntity.Expires,
            Revoked = tokenEntity.Revoked,
            UserId = tokenEntity.UserId
        };
    }

    public async Task InvalidateAsync(string token)
    {
        var tokenEntity = await _authDbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == token);

        if (tokenEntity == null)
            return;

        tokenEntity.Revoked = DateTime.UtcNow;

        await _authDbContext.SaveChangesAsync();
    }

    public async Task RemoveExpiredTokensAsync()
    {
        var expiredTokens = await _authDbContext.RefreshTokens
            .Where(t => t.Expires < DateTime.UtcNow)
            .ToListAsync();

        if (!expiredTokens.Any())
            return;

        _authDbContext.RefreshTokens.RemoveRange(expiredTokens);

        await _authDbContext.SaveChangesAsync();
    }

    public async Task InvalidateAllAsync(Guid userId)
    {
        var tokens = await _authDbContext.RefreshTokens
            .Where(x => x.UserId == userId && x.Revoked == null)
            .ToListAsync();

        if (!tokens.Any())
            return;

        var now = DateTime.UtcNow;

        foreach (var token in tokens)
        {
            token.Revoked = now;
        }

        await _authDbContext.SaveChangesAsync();
    }
}