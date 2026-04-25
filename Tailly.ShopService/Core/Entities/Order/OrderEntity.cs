using Tailly.ShopService.Core.Entities.Pickup;
using Tailly.ShopService.Core.Enums;

namespace Tailly.ShopService.Core.Entities.Order;

public class OrderEntity
{
    public Guid Id { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string Number { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public decimal TotalPrice { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string RecipientFirstName { get; set; } = string.Empty;
    public string RecipientLastName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public PickupPointEntity? PickupPoint { get; set; }
    public ICollection<OrderItemEntity> Items { get; set; } = [];
    public OrderAddressEntity? Address { get; set; }
}