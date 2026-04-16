using Microsoft.EntityFrameworkCore;
using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;
using Tailly.PostsService.Infrastructure.DataAccess;
using Tailly.PostsService.Infrastructure.Mappers;
using Tailly.PostsService.Infrastructure.Repositories.Interfaces;

namespace Tailly.PostsService.Infrastructure.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly PostDbContext _context;

    public BannerRepository(PostDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Banner banner)
    {
        var entity = BannerEntityMapper.ToEntity(banner);

        await _context.Banners.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Banner banner)
    {
        var entity = await _context.Banners
            .FirstOrDefaultAsync(x => x.Id == banner.Id);

        if (entity == null)
            return;

        entity.Title = banner.Title;
        entity.Description = banner.Description;
        entity.ImageUrl = banner.ImageUrl;
        entity.LinkUrl = banner.LinkUrl;
        entity.LinkTarget = banner.LinkTarget;
        entity.Placement = banner.Placement;
        entity.Status = banner.Status;
        entity.StartsAt = banner.StartsAt;
        entity.EndsAt = banner.EndsAt;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Banners
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return;

        _context.Banners.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<Banner?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Banners
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return entity == null
            ? null
            : BannerEntityMapper.ToDomain(entity);
    }

    public async Task<List<Banner>> GetAllAsync()
    {
        var entities = await _context.Banners
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return entities
            .Select(BannerEntityMapper.ToDomain)
            .ToList();
    }

    public async Task<(List<Banner> banners, int total)> GetListAsync(
        int page,
        int limit,
        string? status,
        string? placement,
        BannerSort? sort)
    {
        var query = _context.Banners
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<BannerStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(placement) &&
            Enum.TryParse<BannerPlacement>(placement, true, out var parsedPlacement))
        {
            query = query.Where(x => x.Placement == parsedPlacement);
        }

        query = sort switch
        {
            BannerSort.Oldest => query.OrderBy(x => x.CreatedAt),
            BannerSort.TitleAsc => query.OrderBy(x => x.Title),
            BannerSort.TitleDesc => query.OrderByDescending(x => x.Title),
            BannerSort.StartsAtAsc => query.OrderBy(x => x.StartsAt ?? DateTime.MaxValue),
            BannerSort.StartsAtDesc => query.OrderByDescending(x => x.StartsAt ?? DateTime.MinValue),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var total = await query.CountAsync();

        var entities = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (
            entities.Select(BannerEntityMapper.ToDomain).ToList(),
            total
        );
    }

    public async Task<List<Banner>> GetActiveBannersAsync(DateTime now)
    {
        var entities = await _context.Banners
            .AsNoTracking()
            .Where(x =>
                x.Status == BannerStatus.Published &&
                (x.StartsAt == null || x.StartsAt <= now) &&
                (x.EndsAt == null || x.EndsAt >= now))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return entities
            .Select(BannerEntityMapper.ToDomain)
            .ToList();
    }

    public async Task<List<Banner>> GetBannersByPlacementAsync(BannerPlacement placement, DateTime now)
    {
        var entities = await _context.Banners
            .AsNoTracking()
            .Where(x =>
                x.Status == BannerStatus.Published &&
                (x.StartsAt == null || x.StartsAt <= now) &&
                (x.EndsAt == null || x.EndsAt >= now) &&
                x.Placement == placement)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return entities
            .Select(BannerEntityMapper.ToDomain)
            .ToList();
    }
}