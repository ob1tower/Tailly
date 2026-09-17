using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Tailly.PostsService.Core.Entities;
using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;
using Tailly.PostsService.Infrastructure.DataAccess;
using Tailly.PostsService.Infrastructure.Mappers;
using Tailly.PostsService.Infrastructure.Repositories.Interfaces;

namespace Tailly.PostsService.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly PostDbContext _context;

    public PostRepository(PostDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Post post)
    {
        var entity = PostEntityMapper.ToEntity(post);

        var tagNames = post.Tags
            .Select(t => t.Trim().ToLower())
            .ToList();

        var existingTags = await _context.Tags
            .Where(t => tagNames.Contains(t.Name))
            .ToListAsync();

        var existingNames = existingTags
            .Select(t => t.Name)
            .ToHashSet();

        var newTags = tagNames
            .Where(name => !existingNames.Contains(name))
            .Select(name => new TagEntity
            {
                Id = Guid.NewGuid(),
                Name = name
            })
            .ToList();

        if (newTags.Any())
            _context.Tags.AddRange(newTags);

        var allTags = existingTags.Concat(newTags).ToList();

        entity.Tags = allTags.Select(tag => new PostTagEntity
        {
            PostId = entity.Id,
            Tag = tag
        }).ToList();

        await _context.Posts.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Posts
            .Include(x => x.Images)
            .Include(x => x.Tags)
            .ThenInclude(x => x.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return entity == null ? null : PostEntityMapper.ToDomain(entity);
    }

    public async Task<Post?> GetByIdForAdminAsync(Guid id)
    {
        var entity = await _context.Posts
            .Include(x => x.Images)
            .Include(x => x.Tags)
            .ThenInclude(x => x.Tag)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id); 

        return entity == null ? null : PostEntityMapper.ToDomain(entity);
    }

    public async Task<(List<Post> posts, int total)> GetListAsync(int page, int limit, string? search,
                                                                  string? tag, PostPublicSort? sort)
    {
        var query = _context.Posts
            .Include(x => x.Images)
            .Include(x => x.Tags)
            .ThenInclude(x => x.Tag)
            .AsSplitQuery()
            .AsNoTracking();

        search = search?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, $"%{search}%") ||
                EF.Functions.ILike(x.Content, $"%{search}%"));
        }

        tag = tag?.Trim();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(x =>
                x.Tags.Any(t => t.Tag != null && EF.Functions.ILike(t.Tag.Name, tag)));
        }

        query = sort switch
        {
            PostPublicSort.Oldest => query.OrderBy(x => x.PublishedAt ?? x.CreatedAt),
            PostPublicSort.TitleAsc => query.OrderBy(x => x.Title),
            PostPublicSort.TitleDesc => query.OrderByDescending(x => x.Title),
            _ => query.OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
        };

        var total = await query.CountAsync();

        var entities = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var posts = entities
            .Select(PostEntityMapper.ToDomain)
            .ToList();

        return (posts, total);
    }

    public async Task<List<Post>> GetLatestAsync(int limit)
    {
        limit = Math.Clamp(limit, 1, 20);

        var entities = await _context.Posts
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .ThenInclude(t => t.Tag)
            .AsSplitQuery()
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return entities
            .Select(PostEntityMapper.ToDomain)
            .ToList();
    }

    public async Task UpdateAsync(Post post)
    {
        var entity = await _context.Posts
            .Include(x => x.Images)
            .Include(x => x.Tags)
            .ThenInclude(x => x.Tag)
            .FirstOrDefaultAsync(x => x.Id == post.Id);

        if (entity == null)
            return;

        entity.Title = post.Title;
        entity.Content = post.Content;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.PublishedAt ??= DateTime.UtcNow;

        _context.PostImages.RemoveRange(entity.Images);

        var cover = post.CoverImageUrl ?? post.ImageUrls.FirstOrDefault();
        entity.Images = post.ImageUrls
            .Select(x => new PostImageEntity
            {
                PostId = entity.Id,
                Url = x,
                IsCover = x == cover
            }).ToList();

        _context.PostTags.RemoveRange(entity.Tags);

        var tagNames = post.Tags
            .Select(t => t.Trim().ToLower())
            .Distinct()
            .ToList();

        var existingTags = await _context.Tags
            .Where(t => tagNames.Contains(t.Name))
            .ToListAsync();

        entity.Tags = tagNames.Select(tagName =>
        {
            var existing = existingTags.FirstOrDefault(t => t.Name == tagName);

            var tagEntity = existing ?? new TagEntity
            {
                Id = Guid.NewGuid(),
                Name = tagName
            };

            if (existing == null)
                _context.Tags.Add(tagEntity);

            return new PostTagEntity
            {
                PostId = entity.Id,
                Tag = tagEntity
            };
        }).ToList();

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Posts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return;

        _context.Posts.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Post>> GetAllAsync()
    {
        var entities = await _context.Posts
            .Include(x => x.Images)
            .Include(x => x.Tags)
            .ThenInclude(x => x.Tag)
            .AsSplitQuery()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return entities
            .Select(PostEntityMapper.ToDomain)
            .ToList();
    }

    public async Task<List<string>> GetAllTagsAsync()
    {
        return await _context.Posts
            .SelectMany(p => p.Tags.Select(t => t.Tag!.Name))
            .Distinct()
            .ToListAsync();
    }

    public async Task<(List<Post> posts, int total)> GetAdminListAsync(int page, int limit, string? search, PostAdminSort? sort)
    {
        var query = _context.Posts
            .Include(x => x.Images)
            .Include(x => x.Tags)
            .ThenInclude(x => x.Tag)
            .AsSplitQuery()
            .AsNoTracking();

        search = search?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, $"%{search}%"));
        }

        query = sort switch
        {
            PostAdminSort.Oldest => query.OrderBy(x => x.CreatedAt),
            PostAdminSort.TitleAsc => query.OrderBy(x => x.Title),
            PostAdminSort.TitleDesc => query.OrderByDescending(x => x.Title),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var total = await query.CountAsync();

        var entities = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (
            entities.Select(PostEntityMapper.ToDomain).ToList(),
            total
        );
    }
}