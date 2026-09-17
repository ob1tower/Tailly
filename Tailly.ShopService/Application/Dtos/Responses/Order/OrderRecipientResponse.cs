namespace Tailly.ShopService.Application.Dtos.Responses.Order;

public sealed class OrderRecipientResponse
{
    public string FullName { get; set; } = default!;
    public string Phone { get; set; } = default!;
}