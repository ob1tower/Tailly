namespace Tailly.ShopService.Core.Entities.Cart;

public class CartItemEntity
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public CartEntity? Cart { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}