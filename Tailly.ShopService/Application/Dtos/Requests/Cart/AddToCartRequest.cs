namespace Tailly.ShopService.Application.Dtos.Requests.Cart;

public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}