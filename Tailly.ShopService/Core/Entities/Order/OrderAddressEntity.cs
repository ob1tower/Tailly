namespace Tailly.ShopService.Core.Entities.Order;

public class OrderAddressEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderEntity? Order { get; set; }
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string House { get; set; } = string.Empty;
    public string Apartment { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}