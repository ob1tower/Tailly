namespace Tailly.ShopService.Application.Dtos.Requests.Order;

public sealed class OrderAddress
{
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string House { get; set; } = default!;
    public string Apartment { get; set; } = default!;
    public string Comment { get; set; } = default!;
}