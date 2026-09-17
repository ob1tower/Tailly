using CSharpFunctionalExtensions;
using Tailly.PostsService.Application.Errors;
using Tailly.PostsService.Application.Mappers;
using Tailly.PostsService.Application.Service.Interfaces;
using Tailly.PostsService.Core.Common;
using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;
using Tailly.PostsService.Infrastructure.Repositories.Interfaces;

namespace Tailly.PostsService.Application.Service;

public class PostService : IPostService
{
    private readonly IPostRepository _repository;
    private readonly ILogger<PostService> _logger;

    public PostService(IPostRepository repository, ILogger<PostService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Post, Error>> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("GetByIdAsync failed. Empty id.");
            return Result.Failure<Post, Error>(PostErrors.PostNotFound);
        }

        var post = await _repository.GetByIdAsync(id);

        if (post == null)
        {
            _logger.LogWarning("Post not found. Id: {PostId}", id);
            return Result.Failure<Post, Error>(PostErrors.PostNotFound);
        }

        _logger.LogInformation("Post retrieved successfully. Id: {PostId}", id);
        return Result.Success<Post, Error>(post);
    }

    public async Task<Result<List<Post>, Error>> GetAllListAsync()
    {
        var posts = await _repository.GetAllAsync();

        _logger.LogInformation("Retrieved {Count} posts for admin panel", posts.Count);

        return Result.Success<List<Post>, Error>(posts);
    }

    public async Task<Result<(List<Post> posts, int total, List<string> tags), Error>> GetListAsync(int page, int limit, string? search, string? tag, string? sort)
    {
        PostPublicSort? parsedSort = null;

        if (!string.IsNullOrWhiteSpace(sort))
        {
            if (!PostMapper.TryParsePublicSort(sort, out var sortValue))
            {
                _logger.LogWarning("GetListAsync failed. Invalid sort: {Sort}", sort);
                return Result.Failure<(List<Post>, int, List<string>), Error>(PostErrors.InvalidSort);
            }

            parsedSort = sortValue;
        }

        var (posts, total) = await _repository.GetListAsync(
            page,
            limit,
            search,
            tag,
            parsedSort);

        var tags = await _repository.GetAllTagsAsync();

        _logger.LogInformation(
            "Retrieved {Count} published posts (page {Page})",
            posts.Count,
            page);

        return Result.Success<(List<Post>, int, List<string>), Error>((posts, total, tags));
    }

    public async Task<Result<List<Post>, Error>> GetLatestAsync(int limit)
    {
        limit = Math.Clamp(limit, 1, 20);

        var posts = await _repository.GetLatestAsync(limit);

        _logger.LogInformation("Retrieved {Count} latest published posts", posts.Count);

        return Result.Success<List<Post>, Error>(posts);
    }

    public async Task<Result<Post, Error>> CreateAsync(Post post)
    {
        if (post == null)
        {
            _logger.LogWarning("CreateAsync failed. Post is null.");
            return Result.Failure<Post, Error>(PostErrors.InvalidPost);
        }

        if (string.IsNullOrWhiteSpace(post.Title))
        {
            _logger.LogWarning("CreateAsync failed. Title is empty.");
            return Result.Failure<Post, Error>(PostErrors.EmptyTitle);
        }

        if (string.IsNullOrWhiteSpace(post.Content))
        {
            _logger.LogWarning("CreateAsync failed. Content is empty.");
            return Result.Failure<Post, Error>(PostErrors.EmptyContent);
        }

        post.CreatedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        post.PublishedAt = DateTime.UtcNow;

        await _repository.AddAsync(post);

        _logger.LogInformation("Post created successfully. Id: {PostId}", post.Id);

        return Result.Success<Post, Error>(post);
    }

    public async Task<Result<Post, Error>> UpdateAsync(Post post)
    {
        if (post == null || post.Id == Guid.Empty)
        {
            _logger.LogWarning("UpdateAsync failed. Invalid post or empty id.");
            return Result.Failure<Post, Error>(PostErrors.InvalidPost);
        }

        var existing = await _repository.GetByIdForAdminAsync(post.Id);

        if (existing == null)
        {
            _logger.LogWarning("UpdateAsync failed. Post not found: {PostId}", post.Id);
            return Result.Failure<Post, Error>(PostErrors.PostNotFound);
        }

        if (string.IsNullOrWhiteSpace(post.Title))
        {
            _logger.LogWarning("UpdateAsync failed. Title is empty.");
            return Result.Failure<Post, Error>(PostErrors.EmptyTitle);
        }

        if (string.IsNullOrWhiteSpace(post.Content))
        {
            _logger.LogWarning("UpdateAsync failed. Content is empty.");
            return Result.Failure<Post, Error>(PostErrors.EmptyContent);
        }

        post.UpdatedAt = DateTime.UtcNow;
        post.PublishedAt ??= existing.PublishedAt ?? DateTime.UtcNow;

        await _repository.UpdateAsync(post);

        _logger.LogInformation("Post updated successfully. Id: {PostId}", post.Id);

        return Result.Success<Post, Error>(post);
    }

    public async Task<Result<(List<Post>, int), Error>> GetAdminListAsync(int page, int limit, string? search, string? sort)
    {
        PostAdminSort? parsedSort = null;

        if (!string.IsNullOrWhiteSpace(sort))
        {
            if (!PostMapper.TryParseAdminSort(sort, out var sortValue))
            {
                _logger.LogWarning("GetAdminListAsync failed. Invalid sort: {Sort}", sort);
                return Result.Failure<(List<Post>, int), Error>(PostErrors.InvalidSort);
            }

            parsedSort = sortValue;
        }

        var (posts, total) = await _repository.GetAdminListAsync(page, limit, search, parsedSort);

        _logger.LogInformation("Retrieved {Count} posts (admin list). Page: {Page}", posts.Count, page);

        return Result.Success<(List<Post>, int), Error>((posts, total));
    }

    public async Task<Result> DeleteAsync(Guid id, Guid userId)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("DeleteAsync failed. Empty id.");
            return Result.Failure(PostErrors.PostNotFound.Description);
        }

        var existing = await _repository.GetByIdForAdminAsync(id);

        if (existing == null)
        {
            _logger.LogWarning("DeleteAsync failed. Post not found: {PostId}", id);
            return Result.Failure(PostErrors.PostNotFound.Description);
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation(
            "Post deleted successfully. Id: {PostId} by user {UserId}",
            id,
            userId);

        return Result.Success();
    }
}