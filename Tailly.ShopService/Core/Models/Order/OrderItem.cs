namespace Tailly.ShopService.Core.Models.Order;

public class OrderItem
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? ImageUrl { get; set; }
}