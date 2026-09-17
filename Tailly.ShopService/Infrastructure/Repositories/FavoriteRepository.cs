using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Core.Entities.User;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Core.Models.User;
using Tailly.ShopService.Infrastructure.DataAccess;
using Tailly.ShopService.Infrastructure.Mappers;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Infrastructure.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly ShopDbContext _context;

    public FavoriteRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Favorite favorite)
    {
        var entity = favorite.ToEntity();

        await _context.Favorites.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Favorite>> GetAsync(Guid? userId, Guid? sessionId)
    {
        var query = _context.Favorites.AsQueryable();

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId);
        else if (sessionId.HasValue)
            query = query.Where(x => x.SessionId == sessionId);
        else
            return new List<Favorite>();

        var entities = await query.ToListAsync();

        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<bool> ExistsAsync(Guid? userId, Guid? sessionId, Guid productId)
    {
        return await _context.Favorites.AnyAsync(x =>
            x.ProductId == productId &&
            ((userId.HasValue && x.UserId == userId) ||
             (!userId.HasValue && sessionId.HasValue && x.SessionId == sessionId)));
    }

    public async Task RemoveAsync(Guid? userId, Guid? sessionId, Guid productId)
    {
        var entity = await _context.Favorites.FirstOrDefaultAsync(x =>
            x.ProductId == productId &&
            ((userId.HasValue && x.UserId == userId) ||
             (!userId.HasValue && sessionId.HasValue && x.SessionId == sessionId)));

        if (entity == null)
            return;

        _context.Favorites.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ClearAsync(Guid? userId, Guid? sessionId)
    {
        if (userId.HasValue)
        {
            await _context.Favorites
                .Where(x => x.UserId == userId)
                .ExecuteDeleteAsync();
        }
        else if (sessionId.HasValue)
        {
            await _context.Favorites
                .Where(x => x.SessionId == sessionId)
                .ExecuteDeleteAsync();
        }
    }

    public async Task<List<Product>> GetByIdsAsync(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
            return new List<Product>();

        var entities = await _context.Products
            .Where(x => ids.Contains(x.Id))
            .Include(x => x.Images)
            .AsNoTracking()
            .ToListAsync();

        return entities.Select(x => x.ToProductDomain()).ToList();
    }
}