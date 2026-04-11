namespace Tailly.ShopService.Application.Dtos.Requests.Order;

public sealed class OrderRecipient
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
}