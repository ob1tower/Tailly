using Tailly.ShopService.Core.Enums;

namespace Tailly.ShopService.Core.Models.Order.Checkout;

public class CheckoutForm
{
    public CheckoutRecipient Recipient { get; set; } = new();
    public DeliveryMethod DeliveryMethod { get; set; }
    public CheckoutAddress? Address { get; set; }          
    public string? PickupPointId { get; set; }         
    public PaymentMethod PaymentMethod { get; set; }
}