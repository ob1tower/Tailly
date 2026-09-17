namespace Tailly.ShopService.Application.Dtos.Responses.Cart;

public class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = [];
    public int TotalItems { get; set; }
    public decimal TotalPrice { get; set; }
}