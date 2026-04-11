using Tailly.ShopService.Core.Enums;

namespace Tailly.ShopService.Application.Mappers;

public static class ShopMapper
{
    public static string MapProductSort(ProductSort sort) => sort switch
    {
        ProductSort.Popular => "popular",
        ProductSort.PriceAsc => "price-asc",
        ProductSort.PriceDesc => "price-desc",
        ProductSort.RatingDesc => "rating-desc",
        ProductSort.Newest => "newest",
        _ => "newest"
    };

    public static ProductSort ParseProductSort(string? sortValue)
    {
        if (string.IsNullOrWhiteSpace(sortValue))
            return ProductSort.Newest;

        return sortValue.Trim().ToLowerInvariant() switch
        {
            "popular" => ProductSort.Popular,
            "price-asc" => ProductSort.PriceAsc,
            "price-desc" => ProductSort.PriceDesc,
            "rating-desc" => ProductSort.RatingDesc,
            "newest" => ProductSort.Newest,
            _ => ProductSort.Newest
        };
    }

    public static string MapDeliveryMethod(DeliveryMethod method) => method switch
    {
        DeliveryMethod.Courier => "courier",
        DeliveryMethod.PickupPoint => "pickup-point",
        _ => "courier"
    };

    public static DeliveryMethod ParseDeliveryMethod(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DeliveryMethod.Courier;

        return value.Trim().ToLowerInvariant() switch
        {
            "courier" => DeliveryMethod.Courier,
            "pickup" or "pickuppont" or "pickup-point" or "pickuppoint" => DeliveryMethod.PickupPoint,
            _ => DeliveryMethod.Courier
        };
    }

    public static string MapPaymentMethod(PaymentMethod method) => method switch
    {
        PaymentMethod.Card => "card",
        PaymentMethod.Sbp => "sbp",
        PaymentMethod.Cash => "cash",
        _ => "card"
    };

    public static PaymentMethod ParsePaymentMethod(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return PaymentMethod.Card;

        return value.Trim().ToLowerInvariant() switch
        {
            "card" => PaymentMethod.Card,
            "sbp" => PaymentMethod.Sbp,
            "cash" => PaymentMethod.Cash,
            _ => PaymentMethod.Card
        };
    }

    public static string MapOrderStatus(OrderStatus status) => status switch
    {
        OrderStatus.Created => "created",
        OrderStatus.Paid => "paid",
        OrderStatus.Processing => "processing",
        OrderStatus.Delivering => "delivering",
        OrderStatus.ReadyForPickup => "ready-for-pickup",
        OrderStatus.Completed => "completed",
        OrderStatus.Cancelled => "cancelled",
        _ => "created"
    };

    public static string MapPickupProvider(PickupProvider provider) => provider switch
    {
        PickupProvider.Cdek => "cdek",
        _ => "cdek"
    };
}