using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Core.Entities.Product;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Infrastructure.DataAccess;
using Tailly.ShopService.Infrastructure.Mappers;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ShopDbContext _context;

    public ProductRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews).ThenInclude(r => r.Reply)
            .Include(p => p.Reviews).ThenInclude(r => r.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return entity == null ? null : ProductEntityMapper.ToDomain(entity);
    }

    public async Task<Product?> GetBySlugAsync(string slug, ReviewSortType reviewSort)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsNoTracking()
            .AsQueryable();

        query = reviewSort switch
        {
            ReviewSortType.Newest =>
                query.Include(p => p.Reviews
                    .OrderByDescending(r => r.CreatedAt))
                    .ThenInclude(r => r.Reply)
                    .Include(p => p.Reviews
                        .OrderByDescending(r => r.CreatedAt))
                    .ThenInclude(r => r.Images),

            ReviewSortType.Oldest =>
                query.Include(p => p.Reviews
                    .OrderBy(r => r.CreatedAt))
                    .ThenInclude(r => r.Reply)
                    .Include(p => p.Reviews
                        .OrderBy(r => r.CreatedAt))
                    .ThenInclude(r => r.Images),

            ReviewSortType.Positive =>
                query.Include(p => p.Reviews
                    .OrderByDescending(r => r.Rating)
                    .ThenByDescending(r => r.CreatedAt))
                    .ThenInclude(r => r.Reply)
                    .Include(p => p.Reviews
                        .OrderByDescending(r => r.Rating)
                        .ThenByDescending(r => r.CreatedAt))
                    .ThenInclude(r => r.Images),

            ReviewSortType.Negative =>
                query.Include(p => p.Reviews
                    .OrderBy(r => r.Rating)
                    .ThenByDescending(r => r.CreatedAt))
                    .ThenInclude(r => r.Reply)
                    .Include(p => p.Reviews
                        .OrderBy(r => r.Rating)
                        .ThenByDescending(r => r.CreatedAt))
                    .ThenInclude(r => r.Images),

            _ =>
                query.Include(p => p.Reviews
                    .OrderByDescending(r => r.CreatedAt))
                    .ThenInclude(r => r.Reply)
                    .Include(p => p.Reviews
                        .OrderByDescending(r => r.CreatedAt))
                    .ThenInclude(r => r.Images)
        };

        var entity = await query
            .FirstOrDefaultAsync(p => p.Slug == slug);

        return entity == null
            ? null
            : ProductEntityMapper.ToDomain(entity);
    }

    public async Task<List<Product>> GetByIdsAsync(List<Guid> ids)
    {
        if (ids == null || !ids.Any())
            return new List<Product>();

        var entities = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        return entities.Select(ProductEntityMapper.ToDomain).ToList();
    }

    public async Task<(List<Product> products, int total)> GetCatalogAsync(string? search, List<string>? categoryIds, decimal? minPrice, decimal? maxPrice, bool onlyAvailable, ProductSort sort, int page, int limit)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Take(1))
            .Include(p => p.Reviews)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm) ||
                p.ShortDescription.ToLower().Contains(searchTerm));
        }

        if (categoryIds != null && categoryIds.Any())
        {
            var categoryGuids = categoryIds
                .SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(x => x.Trim())
                .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();

            if (categoryGuids.Any())
                query = query.Where(p => categoryGuids.Contains(p.CategoryId));
        }

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (onlyAvailable)
            query = query.Where(p => p.IsAvailable && p.StockQuantity > 0);

        query = sort switch
        {
            ProductSort.PriceAsc => query.OrderBy(p => p.Price),
            ProductSort.PriceDesc => query.OrderByDescending(p => p.Price),
            ProductSort.RatingDesc => query.OrderByDescending(p => p.Rating),
            ProductSort.Newest => query.OrderByDescending(p => p.CreatedAt),
            ProductSort.Popular => query.OrderByDescending(p => p.ReviewsCount),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var entities = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var products = entities.Select(ProductEntityMapper.ToDomain).ToList();

        return (products, total);
    }

    public async Task<(List<Category> categories, decimal minPrice, decimal maxPrice)> GetCatalogMetaAsync()
    {
        var categoryEntities = await _context.Categories
            .AsNoTracking()
            .ToListAsync();

        var priceStats = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsAvailable)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                MinPrice = g.Min(p => p.Price),
                MaxPrice = g.Max(p => p.Price)
            })
            .FirstOrDefaultAsync();

        var categories = categoryEntities.Select(c => new Category
        {
            Id = c.Id,
            Slug = c.Slug,
            Title = c.Title
        }).ToList();

        return (categories, priceStats?.MinPrice ?? 0m, priceStats?.MaxPrice ?? 0m);
    }

    public async Task<ProductReview?> GetReviewByIdAsync(Guid reviewId)
    {
        var entity = await _context.ProductReviews
            .Include(r => r.Reply)
            .Include(r => r.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        return entity?.ToDomain();   
    }

    public async Task AddReviewReplyAsync(Guid reviewId, ProductReviewReply reply)
    {
        var replyEntity = new ProductReviewReplyEntity
        {
            Id = reply.Id,
            ReviewId = reviewId,
            AuthorName = reply.AuthorName,
            Text = reply.Text,
            CreatedAt = reply.CreatedAt
        };

        _context.ProductReviewReplys.Add(replyEntity);
        await _context.SaveChangesAsync();
    }

    public async Task AddReviewAsync(ProductReview review)
    {
        var entity = new ProductReviewEntity
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            OrderId = review.OrderId,
            AuthorName = review.AuthorName,
            Rating = review.Rating,
            Text = review.Text,
            CreatedAt = review.CreatedAt,

            Images = review.Images.Select(x => new ProductReviewImageEntity
            {
                Id = x.Id,
                Url = x.Url
            }).ToList()
        };

        _context.ProductReviews.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasReviewAsync(Guid userId, Guid orderId, Guid productId)
    {
        return await _context.ProductReviews
            .AnyAsync(r => r.UserId == userId &&
                           r.OrderId == orderId &&
                           r.ProductId == productId);
    }

    public async Task UpdateStockAsync(Guid productId, int stockQuantity)
    {
        var entity = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == productId);

        if (entity == null)
            return;

        entity.StockQuantity = stockQuantity;

        await _context.SaveChangesAsync();
    }
}