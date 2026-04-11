namespace Tailly.ShopService.Application.Dtos.Responses.Order;

public sealed class OrderDeliveryResponse
{
    public string Method { get; set; } = default!;
    public OrderAddressResponse? Address { get; set; }
    public string? PickupPointLabel { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ExpectedAt { get; set; }
}