using Tailly.ShopService.Application.Dtos.Responses.Order;
using Tailly.ShopService.Application.Dtos.Responses.Product;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Order;
using Tailly.ShopService.Core.Models.Order.Checkout;

namespace Tailly.ShopService.Application.Mappers;

public static class OrderResponseMapper
{
    public static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id.ToString(),
            Number = order.Number,
            Status = ShopMapper.MapOrderStatus(order.Status),
            CreatedAt = order.CreatedAt,
            Price = order.TotalPrice,
            Currency = "RUB",
            ItemsCount = order.Items.Count,

            ProductThumbs = order.Items
                .Select(i => i.ImageUrl)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList()!,

            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId.ToString(),
                Title = i.ProductTitle,
                Quantity = i.Quantity,
                Price = i.Price,
                ImageUrl = i.ImageUrl,                   

                Images = string.IsNullOrEmpty(i.ImageUrl)
                    ? new List<ProductImageResponse>()
                    : new List<ProductImageResponse>
                    {
                        new ProductImageResponse
                        {
                            Id = Guid.NewGuid().ToString(),                    
                            Url = i.ImageUrl!,
                            Alt = i.ProductTitle
                        }
                    }
            }).ToList(),

            Recipient = new OrderRecipientResponse
            {
                FullName = $"{order.RecipientFirstName} {order.RecipientLastName}".Trim(),
                Phone = order.RecipientPhone
            },

            Delivery = new OrderDeliveryResponse
            {
                Method = ShopMapper.MapDeliveryMethod(order.DeliveryMethod),
                Address = order.Address != null ? new OrderAddressResponse
                {
                    City = order.Address.City,
                    Street = order.Address.Street,
                    House = order.Address.House,
                    Apartment = order.Address.Apartment,
                    Comment = order.Address.Comment
                } : null,
                PickupPointLabel = order.PickupPoint?.Title,
                ExpectedAt = order.EstimatedDeliveryDate,
                TrackingNumber = order.TrackingNumber
            },

            Payment = new OrderPaymentResponse
            {
                Method = ShopMapper.MapPaymentMethod(order.PaymentMethod),
                Status = order.Status == OrderStatus.Paid ? "paid" : "pending"
            }
        };
    }

    public static OrderRepeatCheckoutDraftResponse ToRepeatDraftResponse(OrderRepeatCheckoutDraft draft)
    {
        return new OrderRepeatCheckoutDraftResponse
        {
            Source = draft.Source,
            OrderId = draft.OrderId.ToString(),
            CreatedAt = draft.CreatedAt,
            Items = draft.Items.Select(i => new OrderRepeatCheckoutItemResponse
            {
                ProductId = i.ProductId.ToString(),
                Title = i.Title,
                Quantity = i.Quantity,
                Price = i.Price,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }
}