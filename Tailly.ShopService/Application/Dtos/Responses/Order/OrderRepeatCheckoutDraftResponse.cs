namespace Tailly.ShopService.Application.Dtos.Responses.Order;

public sealed class OrderRepeatCheckoutDraftResponse
{
    public string Source { get; set; } = "repeat_product_order";
    public string OrderId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public List<OrderRepeatCheckoutItemResponse> Items { get; set; } = [];
}