namespace Tailly.ShopService.Core.Models.Order;

public class OrderAddress
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string House { get; set; } = string.Empty;
    public string Apartment { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}