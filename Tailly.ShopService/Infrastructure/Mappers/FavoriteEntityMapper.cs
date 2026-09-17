using Tailly.ShopService.Core.Entities.Product;
using Tailly.ShopService.Core.Entities.User;
using Tailly.ShopService.Core.Models.Products;
using Tailly.ShopService.Core.Models.User;

namespace Tailly.ShopService.Infrastructure.Mappers;

public static class FavoriteEntityMapper
{
    public static Favorite ToDomain(this FavoriteEntity entity)
    {
        return new Favorite
        {
            Id = entity.Id,
            UserId = entity.UserId,
            SessionId = entity.SessionId,
            ProductId = entity.ProductId,
            AddedAt = entity.AddedAt
        };
    }

    public static FavoriteEntity ToEntity(this Favorite model)
    {
        return new FavoriteEntity
        {
            Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
            UserId = model.UserId,
            SessionId = model.SessionId,
            ProductId = model.ProductId,
            AddedAt = model.AddedAt
        };
    }

    public static Product ToProductDomain(this ProductEntity entity)
    {
        return new Product
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            Price = entity.Price,
            OldPrice = entity.OldPrice,
            Images = entity.Images.Select(img => img.ToDomain()).ToList()
        };
    }

    public static ProductImage ToDomain(this ProductImageEntity entity)
    {
        return new ProductImage
        {
            Id = entity.Id,
            Url = entity.Url,
            Alt = entity.Alt
        };
    }
}