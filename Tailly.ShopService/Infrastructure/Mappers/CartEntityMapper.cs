using Tailly.ShopService.Core.Entities.Cart;
using Tailly.ShopService.Core.Models.Cart;

namespace Tailly.ShopService.Infrastructure.Mappers;

public static class CartEntityMapper
{
    public static Cart? ToDomain(this CartEntity? entity)
    {
        if (entity == null)
            return null;

        return new Cart
        {
            Id = entity.Id,
            UserId = entity.UserId,
            SessionId = entity.SessionId,
            Items = entity.Items.Select(x => x.ToDomain()).ToList()
        };
    }

    public static CartItem ToDomain(this CartItemEntity entity)
    {
        return new CartItem
        {
            ProductId = entity.ProductId,
            ProductTitle = entity.ProductTitle,
            ProductSlug = entity.ProductSlug,
            ImageUrl = entity.ImageUrl,
            Price = entity.Price,
            OldPrice = entity.OldPrice,
            Quantity = entity.Quantity,
            LineTotal = entity.Price * entity.Quantity
        };
    }

    public static CartItemEntity ToEntity(this CartItem model, Guid cartId)
    {
        return new CartItemEntity
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = model.ProductId,
            ProductTitle = model.ProductTitle,
            ProductSlug = model.ProductSlug,
            ImageUrl = model.ImageUrl,
            Price = model.Price,
            OldPrice = model.OldPrice,
            Quantity = model.Quantity,
            UpdatedAt = DateTime.UtcNow
        };
    }
}