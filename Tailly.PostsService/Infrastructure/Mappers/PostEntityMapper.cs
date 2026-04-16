using Tailly.PostsService.Core.Entities;
using Tailly.PostsService.Core.Models;

namespace Tailly.PostsService.Infrastructure.Mappers;

public static class PostEntityMapper
{
    public static Post ToDomain(PostEntity entity)
    {
        return new Post
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            PublishedAt = entity.PublishedAt,
            CreatedBy = entity.CreatedBy,
            CoverImageUrl = entity.Images
                .FirstOrDefault(x => x.IsCover)?.Url
                ?? entity.Images.FirstOrDefault()?.Url,
            ImageUrls = entity.Images
                .Select(x => x.Url)
                .ToList(),
            Tags = entity.Tags
                .Select(x => x.Tag!.Name)
                .ToList()
        };
    }

    public static PostEntity ToEntity(Post model)
    {
        var cover = model.CoverImageUrl ?? model.ImageUrls.FirstOrDefault();

        return new PostEntity
        {
            Id = model.Id,
            Title = model.Title,
            Content = model.Content,
            Status = model.Status,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            PublishedAt = model.PublishedAt,
            CreatedBy = model.CreatedBy,

            Images = model.ImageUrls
                .Select(x => new PostImageEntity
                {
                    Url = x,
                    IsCover = x == cover
                }).ToList(),

            Tags = new List<PostTagEntity>()
        };
    }
}