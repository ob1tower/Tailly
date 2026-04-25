using Tailly.ShopService.Core.Enums;

namespace Tailly.ShopService.Application.Helpers;

public static class OrderStatusCalculator
{
    public static OrderStatus Calculate(DateTime createdAt, bool isCancelled)
    {
        if (isCancelled)
            return OrderStatus.Cancelled;

        var randomHours = (createdAt.Day * 7) % 24; 

        var adjustedNow = DateTime.UtcNow.AddHours(-randomHours);

        var daysPassed = (adjustedNow.Date - createdAt.Date).Days;

        return daysPassed switch
        {
            <= 0 => OrderStatus.Created,
            1 => OrderStatus.Assembled,
            2 => OrderStatus.Shipped,
            >= 3 => OrderStatus.Completed
        };
    }
}