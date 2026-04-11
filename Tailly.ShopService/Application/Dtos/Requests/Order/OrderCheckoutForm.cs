namespace Tailly.ShopService.Application.Dtos.Requests.Order;

public sealed class OrderCheckoutForm
{
    public OrderRecipient Recipient { get; set; } = default!;
    public string DeliveryMethod { get; set; } = default!;   
    public OrderAddress? Address { get; set; }
    public string? PickupPointId { get; set; }
    public string PaymentMethod { get; set; } = default!;    
}