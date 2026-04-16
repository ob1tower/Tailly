using CSharpFunctionalExtensions;
using Tailly.PostsService.Application.Errors;
using Tailly.PostsService.Application.Mappers;
using Tailly.PostsService.Application.Service.Interfaces;
using Tailly.PostsService.Core.Common;
using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;
using Tailly.PostsService.Infrastructure.Repositories.Interfaces;

namespace Tailly.PostsService.Application.Service;

public class BannerService : IBannerService
{
    private readonly IBannerRepository _repository;
    private readonly ILogger<BannerService> _logger;

    public BannerService(IBannerRepository repository, ILogger<BannerService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Banner, Error>> CreateAsync(Banner banner)
    {
        if (banner == null)
        {
            _logger.LogWarning("CreateAsync failed: banner is null");
            return Result.Failure<Banner, Error>(BannerErrors.InvalidBanner);
        }

        if (string.IsNullOrWhiteSpace(banner.Title))
        {
            _logger.LogWarning("CreateAsync failed: title is empty");
            return Result.Failure<Banner, Error>(BannerErrors.EmptyTitle);
        }

        if (string.IsNullOrWhiteSpace(banner.Description))
        {
            _logger.LogWarning("CreateAsync failed: description is empty");
            return Result.Failure<Banner, Error>(BannerErrors.EmptyDescription);
        }

        banner.CreatedAt = DateTime.UtcNow;
        banner.UpdatedAt = DateTime.UtcNow;

        if (banner.Status == BannerStatus.Published)
        {
            banner.StartsAt ??= DateTime.UtcNow;
        }

        await _repository.AddAsync(banner);

        _logger.LogInformation("Banner created successfully. Id: {BannerId}, Status: {Status}",
            banner.Id, banner.Status);

        return Result.Success<Banner, Error>(banner);
    }

    public async Task<Result<Banner, Error>> UpdateAsync(Banner banner)
    {
        if (banner == null || banner.Id == Guid.Empty)
        {
            _logger.LogWarning("UpdateAsync failed: invalid banner or empty id");
            return Result.Failure<Banner, Error>(BannerErrors.InvalidBanner);
        }

        var existing = await _repository.GetByIdAsync(banner.Id);
        if (existing == null)
        {
            _logger.LogWarning("UpdateAsync failed: banner not found. Id: {BannerId}", banner.Id);
            return Result.Failure<Banner, Error>(BannerErrors.BannerNotFound);
        }

        if (string.IsNullOrWhiteSpace(banner.Title))
        {
            _logger.LogWarning("UpdateAsync failed: title is empty");
            return Result.Failure<Banner, Error>(BannerErrors.EmptyTitle);
        }

        if (string.IsNullOrWhiteSpace(banner.Description))
        {
            _logger.LogWarning("UpdateAsync failed: description is empty");
            return Result.Failure<Banner, Error>(BannerErrors.EmptyDescription);
        }

        banner.UpdatedAt = DateTime.UtcNow;

        if (banner.Status == BannerStatus.Published && banner.StartsAt == null)
        {
            banner.StartsAt = DateTime.UtcNow;
        }

        await _repository.UpdateAsync(banner);

        _logger.LogInformation("Banner updated successfully. Id: {BannerId}, New Status: {Status}",
            banner.Id, banner.Status);

        return Result.Success<Banner, Error>(banner);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("DeleteAsync failed: empty id");
            return Result.Failure(BannerErrors.BannerNotFound.Description);
        }

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            _logger.LogWarning("DeleteAsync failed: banner not found. Id: {BannerId}", id);
            return Result.Failure(BannerErrors.BannerNotFound.Description);
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("Banner deleted successfully. Id: {BannerId}", id);
        return Result.Success();
    }

    public async Task<Result<Banner, Error>> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("GetByIdAsync failed: empty id");
            return Result.Failure<Banner, Error>(BannerErrors.BannerNotFound);
        }

        var banner = await _repository.GetByIdAsync(id);

        if (banner == null)
        {
            _logger.LogWarning("Banner not found. Id: {BannerId}", id);
            return Result.Failure<Banner, Error>(BannerErrors.BannerNotFound);
        }

        _logger.LogInformation("Banner retrieved. Id: {BannerId}", id);
        return Result.Success<Banner, Error>(banner);
    }

    public async Task<Result<(List<Banner>, int), Error>> GetListAsync(int page, int limit, string? status, string? placement, string? sort)
    {
        BannerSort? parsedSort = null;

        if (!string.IsNullOrWhiteSpace(sort))
        {
            if (!BannerMapper.TryParseSort(sort, out var sortValue))
            {
                _logger.LogWarning("GetListAsync failed. Invalid sort: {Sort}", sort);
                return Result.Failure<(List<Banner>, int), Error>(BannerErrors.InvalidSort);
            }

            parsedSort = sortValue;
        }

        var (banners, total) = await _repository.GetListAsync(
            page,
            limit,
            status,
            placement,
            parsedSort);

        _logger.LogInformation(
            "Retrieved {Count} banners (admin list). Page: {Page}, PageSize: {PageSize}",
            banners.Count,
            page,
            limit);

        return Result.Success<(List<Banner>, int), Error>((banners, total));
    }


    public async Task<Result<List<Banner>, Error>> GetAllAsync()
    {
        var banners = await _repository.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} banners for admin", banners.Count);
        return Result.Success<List<Banner>, Error>(banners);
    }

    public async Task<Result<List<Banner>, Error>> GetActiveBannersAsync()
    {
        var now = DateTime.UtcNow;

        var banners = await _repository.GetActiveBannersAsync(now);

        _logger.LogInformation("Retrieved {Count} active banners for public", banners.Count);

        return Result.Success<List<Banner>, Error>(banners);
    }

    public async Task<Result<List<Banner>, Error>> GetBannersByPlacementAsync(string placement)
    {
        if (!BannerMapper.TryParsePlacement(placement, out var parsedPlacement))
        {
            _logger.LogWarning("Invalid placement: {Placement}", placement);
            return Result.Failure<List<Banner>, Error>(BannerErrors.InvalidPlacement);
        }

        var now = DateTime.UtcNow;

        var banners = await _repository.GetBannersByPlacementAsync(parsedPlacement, now);

        _logger.LogInformation("Retrieved {Count} banners for placement '{Placement}'", banners.Count, placement);

        return Result.Success<List<Banner>, Error>(banners ?? new List<Banner>());
    }
}