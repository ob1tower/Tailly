namespace Tailly.ShopService.Core.Models.Order.Checkout;

public class CheckoutAddress
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string House { get; set; } = string.Empty;
    public string Apartment { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}