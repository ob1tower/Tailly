using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Infrastructure.Repositories.Interfaces;

public interface IPostRepository
{
    Task AddAsync(Post post);
    Task DeleteAsync(Guid id);
    Task<Post?> GetByIdAsync(Guid id);
    Task<(List<Post> posts, int total)> GetListAsync(int page, int limit, string? search, string? tag, PostPublicSort? sort);
    Task UpdateAsync(Post post);
    Task<List<Post>> GetLatestAsync(int limit);
    Task<List<string>> GetAllTagsAsync();
    Task<List<Post>> GetAllAsync();
    Task<Post?> GetByIdForAdminAsync(Guid id);
    Task<(List<Post> posts, int total)> GetAdminListAsync(int page, int limit, string? search, PostAdminSort? sort);
}