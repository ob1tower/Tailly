namespace Tailly.BookingService.Core.Enums;

public enum OrderStatus
{
    PendingConfirmation = 1,
    Confirmed,
    Active,
    Completed,
    Canceled
}

public enum PriceUnit
{
    Hour = 1,
    Day,
    Service,
    Walk,
    Visit
}