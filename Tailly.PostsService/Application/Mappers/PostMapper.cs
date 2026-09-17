using Tailly.PostsService.Application.Dtos.Requests;
using Tailly.PostsService.Application.Dtos.Responses;
using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Application.Mappers;

public static class PostMapper
{
    public static PostResponse ToResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CoverImageUrl = post.CoverImageUrl,
            ImageUrls = post.ImageUrls,
            Tags = post.Tags,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            PublishedAt = post.PublishedAt
        };
    }

    public static Post ToModel(CreatePostRequest request, Guid userId)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            Title = (request.Title ?? "").Trim(),
            Content = (request.Content ?? "").Trim(),
            CoverImageUrl = string.IsNullOrWhiteSpace(request.CoverImageUrl) ? null : request.CoverImageUrl.Trim(),

            ImageUrls = request.ImageUrls?
                .Select(url => (url ?? "").Trim())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList() ?? new List<string>(),

            Tags = request.Tags?
                .Select(tag => (tag ?? "").Trim().ToLower())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToList() ?? new List<string>(),

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
    }

    public static Post ToModel(UpdatePostRequest request, Guid id, Guid userId)
    {
        return new Post
        {
            Id = id,
            Title = (request.Title ?? "").Trim(),
            Content = (request.Content ?? "").Trim(),
            CoverImageUrl = string.IsNullOrWhiteSpace(request.CoverImageUrl) ? null : request.CoverImageUrl.Trim(),

            ImageUrls = request.ImageUrls?
                .Select(url => (url ?? "").Trim())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList() ?? new List<string>(),

            Tags = request.Tags?
                .Select(tag => (tag ?? "").Trim().ToLower())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToList() ?? new List<string>(),

            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
    }

    public static bool TryParsePublicSort(string? value, out PostPublicSort sort)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "newest":
                sort = PostPublicSort.Newest;
                return true;

            case "oldest":
                sort = PostPublicSort.Oldest;
                return true;

            case "title_asc":
                sort = PostPublicSort.TitleAsc;
                return true;

            case "title_desc":
                sort = PostPublicSort.TitleDesc;
                return true;

            default:
                sort = default;
                return false;
        }
    }

    public static bool TryParseAdminSort(string? value, out PostAdminSort sort)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "newest":
                sort = PostAdminSort.Newest;
                return true;

            case "oldest":
                sort = PostAdminSort.Oldest;
                return true;

            case "title_asc":
                sort = PostAdminSort.TitleAsc;
                return true;

            case "title_desc":
                sort = PostAdminSort.TitleDesc;
                return true;

            default:
                sort = default;
                return false;
        }
    }
}