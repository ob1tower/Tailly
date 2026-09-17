using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Infrastructure.DataAccess;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Infrastructure.Repositories;

public class AdminProfileRepository : IAdminProfileRepository
{
    private readonly AuthDbContext _context;

    public AdminProfileRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<AdminProfileEntity?> GetByUserIdAsync(Guid userId)
    {
        return await _context.AdminProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(AdminProfileEntity entity)
    {
        await _context.AdminProfiles.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AdminProfileEntity entity)
    {
        var existing = await _context.AdminProfiles
            .FirstOrDefaultAsync(x => x.UserId == entity.UserId);

        if (existing == null)
            return;

        existing.Phone = entity.Phone;
        existing.Department = entity.Department;
        existing.BirthDate = entity.BirthDate;
        existing.LastLoginAt = entity.LastLoginAt;
        existing.PasswordAttemptsLockUntil = entity.PasswordAttemptsLockUntil;
        existing.FailedPasswordAttempts = entity.FailedPasswordAttempts;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid profileId)
    {
        await _context.AdminProfiles
            .Where(p => p.Id == profileId)
            .ExecuteDeleteAsync();
    }
}