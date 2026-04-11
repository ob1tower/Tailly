namespace Tailly.ShopService.Core.Entities.Order;

public class OrderItemEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderEntity? Order { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? ImageUrl { get; set; }
}