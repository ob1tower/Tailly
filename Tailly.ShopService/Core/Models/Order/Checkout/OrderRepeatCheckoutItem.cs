namespace Tailly.ShopService.Core.Models.Order.Checkout;

public class OrderRepeatCheckoutItem
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}