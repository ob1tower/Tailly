namespace Tailly.ShopService.Core.Entities.Cart;

public class CartEntity
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CartItemEntity> Items { get; set; } = [];
}