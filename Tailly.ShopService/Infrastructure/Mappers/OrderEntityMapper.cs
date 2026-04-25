using Tailly.ShopService.Core.Entities.Order;
using Tailly.ShopService.Core.Models.Order;
using Tailly.ShopService.Core.Models.Pickup;

namespace Tailly.ShopService.Infrastructure.Mappers;

public static class OrderEntityMapper
{
    public static OrderEntity ToEntity(Order order)
    {
        var entity = new OrderEntity
        {
            Id = order.Id,
            OwnerUserId = order.OwnerUserId,
            Number = order.Number,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            DeliveryMethod = order.DeliveryMethod,
            PaymentMethod = order.PaymentMethod,
            EstimatedDeliveryDate = order.EstimatedDeliveryDate,
            CreatedAt = order.CreatedAt,

            RecipientFirstName = order.RecipientFirstName,
            RecipientLastName = order.RecipientLastName,
            RecipientPhone = order.RecipientPhone,
            RecipientEmail = order.RecipientEmail
        };

        if (order.Address != null)
        {
            entity.Address = new OrderAddressEntity
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                City = order.Address.City,
                Street = order.Address.Street,
                House = order.Address.House,
                Apartment = order.Address.Apartment ?? "",
                Comment = order.Address.Comment ?? ""
            };
        }

        entity.Items = order.Items?.Select(i => new OrderItemEntity
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = i.ProductId,
            ProductTitle = i.ProductTitle,
            ProductSlug = i.ProductSlug,
            Price = i.Price,
            OldPrice = i.OldPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal,
            ImageUrl = i.ImageUrl
        }).ToList() ?? new List<OrderItemEntity>();

        return entity;
    }

    public static Order ToDomain(OrderEntity entity)
    {
        return new Order
        {
            Id = entity.Id,
            OwnerUserId = entity.OwnerUserId,
            Number = entity.Number,
            Status = entity.Status,
            TotalPrice = entity.TotalPrice,
            DeliveryMethod = entity.DeliveryMethod,
            PaymentMethod = entity.PaymentMethod,
            EstimatedDeliveryDate = entity.EstimatedDeliveryDate,
            CreatedAt = entity.CreatedAt,

            RecipientFirstName = entity.RecipientFirstName,
            RecipientLastName = entity.RecipientLastName,
            RecipientPhone = entity.RecipientPhone,
            RecipientEmail = entity.RecipientEmail,

            PickupPoint = entity.PickupPoint != null ? new PickupPoint
            {
                Id = entity.PickupPoint.Id,
                Title = entity.PickupPoint.Title,
                Address = entity.PickupPoint.Address
            } : null,

            Address = entity.Address != null ? new OrderAddress
            {
                City = entity.Address.City,
                Street = entity.Address.Street,
                House = entity.Address.House,
                Apartment = entity.Address.Apartment,
                Comment = entity.Address.Comment
            } : null,

            Items = entity.Items?
                .Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductTitle = i.ProductTitle,
                    ProductSlug = i.ProductSlug,
                    Price = i.Price,
                    OldPrice = i.OldPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal,
                    ImageUrl = i.ImageUrl
                }).ToList() ?? new List<OrderItem>()
        };
    }
}