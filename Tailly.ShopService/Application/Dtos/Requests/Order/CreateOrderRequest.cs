namespace Tailly.ShopService.Application.Dtos.Requests.Order;

public sealed class CreateOrderRequest
{
    public OrderCheckoutForm Form { get; set; } = default!;
}