using CSharpFunctionalExtensions;
using Tailly.PostsService.Core.Common;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Application.Service.Interfaces;

public interface IBannerService
{
    Task<Result<Banner, Error>> CreateAsync(Banner banner);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<List<Banner>, Error>> GetAllAsync();
    Task<Result<Banner, Error>> GetByIdAsync(Guid id);
    Task<Result<Banner, Error>> UpdateAsync(Banner banner);
    Task<Result<List<Banner>, Error>> GetActiveBannersAsync();
    Task<Result<(List<Banner>, int), Error>> GetListAsync(int page, int limit, string? status, string? placement, string? sort);
    Task<Result<List<Banner>, Error>> GetBannersByPlacementAsync(string placement);
}