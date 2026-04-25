using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Infrastructure.Repositories.Interfaces;

public interface IBannerRepository
{
    Task AddAsync(Banner banner);
    Task DeleteAsync(Guid id);
    Task<List<Banner>> GetAllAsync();
    Task<Banner?> GetByIdAsync(Guid id);
    Task<(List<Banner> banners, int total)> GetListAsync(int page, int limit, string? search, BannerSort? sort);
    Task UpdateAsync(Banner banner);
    Task<List<Banner>> GetActiveBannersAsync(DateTime now);
}