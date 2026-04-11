namespace Tailly.ShopService.Application.Dtos.Responses.Order;

public sealed class OrderResponse
{
    public string Id { get; set; } = default!;
    public string Number { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "RUB";
    public int ItemsCount { get; set; }
    public List<string> ProductThumbs { get; set; } = [];
    public List<OrderItemResponse> Items { get; set; } = [];
    public OrderRecipientResponse? Recipient { get; set; }
    public OrderDeliveryResponse? Delivery { get; set; }
    public OrderPaymentResponse? Payment { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? CanceledAt { get; set; }
}