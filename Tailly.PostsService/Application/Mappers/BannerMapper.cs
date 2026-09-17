using Tailly.PostsService.Application.Dtos.Requests;
using Tailly.PostsService.Application.Dtos.Responses;
using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Application.Mappers;

public static class BannerMapper
{
    public static BannerResponse ToResponse(Banner banner)
    {
        return new BannerResponse
        {
            Id = banner.Id,
            Title = banner.Title,
            Description = banner.Description,
            ImageUrl = banner.ImageUrl,
            LinkUrl = banner.LinkUrl,
            StartsAt = banner.StartsAt,
            EndsAt = banner.EndsAt,
            CreatedAt = banner.CreatedAt,
            UpdatedAt = banner.UpdatedAt
        };
    }

    public static Banner ToModel(SaveBannerRequest request, Guid? id = null)
    {
        return new Banner
        {
            Id = id ?? Guid.NewGuid(),                                 
            Title = (request.Title ?? "").Trim(),
            Description = (request.Description ?? "").Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl)
                ? null
                : request.ImageUrl.Trim(),

            LinkUrl = string.IsNullOrWhiteSpace(request.LinkUrl)
                ? null
                : request.LinkUrl.Trim(),

            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,

            CreatedAt = id == null ? DateTime.UtcNow : default,  
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static bool TryParseSort(string? value, out BannerSort sort)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "newest":
                sort = BannerSort.Newest;
                return true;
            case "oldest":
                sort = BannerSort.Oldest;
                return true;
            case "title_asc":
                sort = BannerSort.TitleAsc;
                return true;
            case "title_desc":
                sort = BannerSort.TitleDesc;
                return true;
            default:
                sort = default;
                return false;
        }
    }
}