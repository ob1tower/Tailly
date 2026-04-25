using Tailly.PostsService.Core.Entities;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Infrastructure.Mappers;

public static class BannerEntityMapper
{
    public static Banner ToDomain(BannerEntity entity)
    {
        return new Banner
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            LinkUrl = entity.LinkUrl,
            StartsAt = entity.StartsAt,
            EndsAt = entity.EndsAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static BannerEntity ToEntity(Banner banner)
    {
        return new BannerEntity
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
}