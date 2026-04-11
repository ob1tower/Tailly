namespace Tailly.ShopService.Application.Dtos.Responses.Cart;

public class CartItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}