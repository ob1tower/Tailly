namespace Tailly.ShopService.Core.Enums;


public enum DeliveryMethod
{
    Courier = 1,
    PickupPoint
}

public enum PaymentMethod
{
    Card = 1,
    Sbp,
    Cash
}

public enum OrderStatus
{
    Created = 1,
    PendingPayment,
    Paid,
    Processing,
    Delivering,
    ReadyForPickup,
    Completed,
    Cancelled
}

public enum PickupProvider
{
    Cdek = 1
}

public enum ProductSort
{
    Popular = 1,
    PriceAsc,
    PriceDesc,
    RatingDesc,
    Newest
}