using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Core.Models.Pickup;
using Tailly.ShopService.Infrastructure.DataAccess;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Infrastructure.Repositories;

public class PickupPointRepository : IPickupPointRepository
{
    private readonly ShopDbContext _context;

    public PickupPointRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<PickupPoint>> GetAllAsync()
    {
        var entities = await _context.PickupPoints
            .AsNoTracking()
            .ToListAsync();

        var result = new List<PickupPoint>();

        foreach (var entity in entities)
        {
            result.Add(new PickupPoint
            {
                Id = entity.Id,
                Provider = entity.Provider,
                Title = entity.Title,
                Address = entity.Address,
                EstimatedDate = entity.EstimatedDate
            });
        }

        return result;
    }

    public async Task<List<PickupPoint>> GetByCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return await GetAllAsync();

        var searchTerm = city.ToLower().Trim();

        var entities = await _context.PickupPoints
            .AsNoTracking()
            .Where(p => p.Address.ToLower().Contains(searchTerm))
            .ToListAsync();

        var result = new List<PickupPoint>();

        foreach (var entity in entities)
        {
            result.Add(new PickupPoint
            {
                Id = entity.Id,
                Provider = entity.Provider,
                Title = entity.Title,
                Address = entity.Address,
                EstimatedDate = entity.EstimatedDate
            });
        }

        return result;
    }

    public async Task<PickupPoint?> GetByIdAsync(Guid id)
    {
        var entity = await _context.PickupPoints
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null)
            return null;

        return new PickupPoint
        {
            Id = entity.Id,
            Provider = entity.Provider,
            Title = entity.Title,
            Address = entity.Address,
            EstimatedDate = entity.EstimatedDate
        };
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.PickupPoints
            .AnyAsync(p => p.Id == id);
    }
}