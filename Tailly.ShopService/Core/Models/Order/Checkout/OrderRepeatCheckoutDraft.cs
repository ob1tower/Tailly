namespace Tailly.ShopService.Core.Models.Order.Checkout;

public class OrderRepeatCheckoutDraft
{
    public string Source { get; set; } = "repeat_product_order";
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderRepeatCheckoutItem> Items { get; set; } = [];
}