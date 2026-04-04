using Microsoft.EntityFrameworkCore;
using Tailly.ClientProfileService.Core.Entities;
using Tailly.ClientProfileService.Core.Models;
using Tailly.ClientProfileService.Infrastructure.DataAccess;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Infrastructure.Repositories;

public class ClientProfileRepository : IClientProfileRepository
{
    private readonly ClientProfileDbContext _context;

    public ClientProfileRepository(ClientProfileDbContext context)
    {
        _context = context;
    }

    public async Task<ClientProfile?> GetByUserIdAsync(Guid userId)
    {
        var entity = await _context.ClientProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (entity == null)
            return null;

        return new ClientProfile
        {
            Id = entity.Id,
            UserId = entity.UserId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            MiddleName = entity.MiddleName,
            Phone = entity.Phone,
            City = entity.City,
            CityId = entity.CityId,
            AvatarUrl = entity.AvatarUrl
        };
    }

    public async Task AddAsync(ClientProfile profile)
    {
        var entity = new ClientProfileEntity
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            MiddleName = profile.MiddleName,
            Phone = profile.Phone,
            City = profile.City,
            CityId = profile.CityId,
            AvatarUrl = profile.AvatarUrl
        };

        await _context.ClientProfiles.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClientProfile profile)
    {
        var entity = await _context.ClientProfiles
            .FirstOrDefaultAsync(x => x.Id == profile.Id);

        if (entity == null)
            return;

        entity.FirstName = profile.FirstName;
        entity.LastName = profile.LastName;
        entity.MiddleName = profile.MiddleName;
        entity.Phone = profile.Phone;
        entity.City = profile.City;
        entity.CityId = profile.CityId;
        entity.AvatarUrl = profile.AvatarUrl;

        await _context.SaveChangesAsync();
    }
}