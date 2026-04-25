using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Pickup;

namespace Tailly.ShopService.Core.Models.Order;

public class Order
{
    public Guid Id { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string Number { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RecipientFirstName { get; set; } = string.Empty;
    public string RecipientLastName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public PickupPoint? PickupPoint { get; set; }
    public OrderAddress? Address { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}