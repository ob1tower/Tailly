using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.DataAccess;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Infrastructure.Repositories;

public class AccountDeletionTokenRepository : IAccountDeletionTokenRepository
{
    private readonly AuthDbContext _context;

    public AccountDeletionTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AccountDeletionToken model)
    {
        var entity = new AccountDeletionTokenEntity
        {
            Token = model.Token,
            UserId = model.UserId,
            ExpiresAt = model.ExpiresAt,
            RoleId = model.RoleId
        };

        await _context.AccountDeletionTokens.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<AccountDeletionToken?> GetAsync(string token)
    {
        var entity = await _context.AccountDeletionTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == token);

        if (entity == null)
            return null;

        return new AccountDeletionToken
        {
            Token = entity.Token,
            UserId = entity.UserId,
            ExpiresAt = entity.ExpiresAt,
            RoleId = entity.RoleId
        };
    }

    public async Task RemoveAsync(string token)
    {
        var entity = await _context.AccountDeletionTokens
            .FirstOrDefaultAsync(x => x.Token == token);

        if (entity == null)
            return;

        _context.AccountDeletionTokens.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveExpiredAsync()
    {
        await _context.AccountDeletionTokens
            .Where(x => x.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }

    public async Task<int> RemoveExpiredCountAsync()
    {
        var deletedCount = await _context.AccountDeletionTokens
            .Where(x => x.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync();

        return deletedCount;
    }
}