namespace Tailly.ShopService.Core.Models.Cart;

public class Cart
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalPrice { get; set; }
    public List<CartItem> Items { get; set; } = [];
}