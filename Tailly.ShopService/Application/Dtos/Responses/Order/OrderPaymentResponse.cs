namespace Tailly.ShopService.Application.Dtos.Responses.Order;

public sealed class OrderPaymentResponse
{
    public string Method { get; set; } = default!;
    public string Status { get; set; } = default!;
}
