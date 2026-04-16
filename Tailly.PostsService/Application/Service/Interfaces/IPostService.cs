using CSharpFunctionalExtensions;
using Tailly.PostsService.Core.Common;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Application.Service.Interfaces;

public interface IPostService
{
    Task<Result<Post, Error>> CreateAsync(Post post);
    Task<Result> DeleteAsync(Guid id, Guid userId);
    Task<Result<Post, Error>> GetByIdAsync(Guid id);
    Task<Result<List<Post>, Error>> GetLatestAsync(int limit);
    Task<Result<(List<Post>, int), Error>> GetAdminListAsync(int page, int limit, string? search, string? status, string? sort);
    Task<Result<Post, Error>> UpdateAsync(Post post);
    Task<Result<List<Post>, Error>> GetAllListAsync();
    Task<Result<(List<Post> posts, int total, List<string> tags), Error>> GetListAsync(int page, int limit, string? search, string? tag, string? sort);
}