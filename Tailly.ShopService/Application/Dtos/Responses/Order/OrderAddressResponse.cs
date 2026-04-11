namespace Tailly.ShopService.Application.Dtos.Responses.Order;

public sealed class OrderAddressResponse
{
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string House { get; set; } = default!;
    public string? Apartment { get; set; }
    public string? Comment { get; set; }
}