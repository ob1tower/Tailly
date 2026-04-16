using Tailly.PostsService.Application.Dtos.Requests;
using Tailly.PostsService.Application.Dtos.Responses;
using Tailly.PostsService.Core.Enums;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Application.Mappers;

public static class BannerMapper
{
    public static string MapStatus(BannerStatus status) => status switch
    {
        BannerStatus.Draft => "draft",
        BannerStatus.Published => "published",
        BannerStatus.Archived => "archived",
        _ => "draft"
    };

    public static bool TryParseStatus(string? value, out BannerStatus status)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "draft":
                status = BannerStatus.Draft;
                return true;
            case "published":
                status = BannerStatus.Published;
                return true;
            case "archived":
                status = BannerStatus.Archived;
                return true;
            default:
                status = default;
                return false;
        }
    }

    public static string MapLinkTarget(BannerLinkTarget target) => target switch
    {
        BannerLinkTarget.Home => "home",
        BannerLinkTarget.Posts => "posts",
        BannerLinkTarget.Specialists => "specialists",
        BannerLinkTarget.Shop => "shop",
        BannerLinkTarget.Profile => "profile",
        _ => "home"
    };

    public static bool TryParseLinkTarget(string? value, out BannerLinkTarget target)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "home":
                target = BannerLinkTarget.Home;
                return true;
            case "posts":
                target = BannerLinkTarget.Posts;
                return true;
            case "specialists":
                target = BannerLinkTarget.Specialists;
                return true;
            case "shop":
                target = BannerLinkTarget.Shop;
                return true;
            case "profile":
                target = BannerLinkTarget.Profile;
                return true;
            default:
                target = default;
                return false;
        }
    }

    public static string MapPlacement(BannerPlacement placement) => placement switch
    {
        BannerPlacement.HomeHero => "home_hero",
        BannerPlacement.Posts => "posts",
        BannerPlacement.Specialists => "specialists",
        BannerPlacement.Shop => "shop",
        _ => "home_hero"
    };

    public static bool TryParsePlacement(string? value, out BannerPlacement placement)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "home_hero":
                placement = BannerPlacement.HomeHero;
                return true;
            case "posts":
                placement = BannerPlacement.Posts;
                return true;
            case "specialists":
                placement = BannerPlacement.Specialists;
                return true;
            case "shop":
                placement = BannerPlacement.Shop;
                return true;
            default:
                placement = default;
                return false;
        }
    }

    public static BannerResponse ToResponse(Banner banner)
    {
        return new BannerResponse
        {
            Id = banner.Id,
            Title = banner.Title,
            Description = banner.Description,
            ImageUrl = banner.ImageUrl,
            LinkUrl = banner.LinkUrl,
            LinkTarget = MapLinkTarget(banner.LinkTarget),
            Placement = MapPlacement(banner.Placement),
            Status = MapStatus(banner.Status),
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

            LinkTarget = TryParseLinkTarget(request.LinkTarget, out var lt) ? lt : BannerLinkTarget.Home,
            Placement = TryParsePlacement(request.Placement, out var pl) ? pl : BannerPlacement.HomeHero,
            Status = TryParseStatus(request.Status, out var st) ? st : BannerStatus.Draft,

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
            case "starts_at_asc":
                sort = BannerSort.StartsAtAsc;
                return true;
            case "starts_at_desc":
                sort = BannerSort.StartsAtDesc;
                return true;
            default:
                sort = default;
                return false;
        }
    }
}