namespace Tailly.ShopService.Application.Dtos.Requests.Order;

public sealed class OrderItemRequest
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
}