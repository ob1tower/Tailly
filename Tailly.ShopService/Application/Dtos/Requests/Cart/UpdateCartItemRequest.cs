namespace Tailly.ShopService.Application.Dtos.Requests.Cart;

public class UpdateCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}