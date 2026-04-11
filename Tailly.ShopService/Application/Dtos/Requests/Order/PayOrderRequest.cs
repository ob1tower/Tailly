namespace Tailly.ShopService.Application.Dtos.Requests.Order;

public sealed class PayOrderRequest
{
    public string PaymentMethod { get; set; } = default!;  
}