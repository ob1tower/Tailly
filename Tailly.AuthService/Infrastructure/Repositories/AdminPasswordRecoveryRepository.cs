using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.DataAccess;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Infrastructure.Repositories;

public class AdminPasswordRecoveryRepository : IAdminPasswordRecoveryRepository
{
    private readonly AuthDbContext _context;

    public AdminPasswordRecoveryRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminPasswordRecovery>> GetAllAsync()
    {
        var entities = await _context.AdminPasswordRecoverys
            .AsNoTracking()
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync();

        var result = new List<AdminPasswordRecovery>();

        foreach (var entity in entities)
        {
            result.Add(new AdminPasswordRecovery
            {
                Id = entity.Id,
                Email = entity.Email,
                RequestedAt = entity.RequestedAt,
                Status = entity.Status,
                ProcessedAt = entity.ProcessedAt,
                TemporaryPassword = entity.TemporaryPassword,
                ProcessedBy = entity.ProcessedBy
            });
        }

        return result;
    }

    public async Task<AdminPasswordRecovery?> GetByIdAsync(Guid id)
    {
        var entity = await _context.AdminPasswordRecoverys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return null;

        return new AdminPasswordRecovery
        {
            Id = entity.Id,
            Email = entity.Email,
            RequestedAt = entity.RequestedAt,
            Status = entity.Status,
            ProcessedAt = entity.ProcessedAt,
            TemporaryPassword = entity.TemporaryPassword,
            ProcessedBy = entity.ProcessedBy
        };
    }

    public async Task AddAsync(AdminPasswordRecovery model)
    {
        var entity = new AdminPasswordRecoveryEntity
        {
            Id = model.Id,
            Email = model.Email,
            RequestedAt = model.RequestedAt,
            Status = model.Status,
            ProcessedAt = model.ProcessedAt,
            TemporaryPassword = model.TemporaryPassword,
            ProcessedBy = model.ProcessedBy
        };

        await _context.AdminPasswordRecoverys.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AdminPasswordRecovery model)
    {
        var entity = await _context.AdminPasswordRecoverys
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (entity == null)
            return;

        entity.Status = model.Status;
        entity.ProcessedAt = model.ProcessedAt;
        entity.TemporaryPassword = model.TemporaryPassword;
        entity.ProcessedBy = model.ProcessedBy;

        await _context.SaveChangesAsync();
    }
}