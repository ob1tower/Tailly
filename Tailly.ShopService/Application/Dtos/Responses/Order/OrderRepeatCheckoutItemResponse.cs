namespace Tailly.ShopService.Application.Dtos.Responses.Order;

public sealed class OrderRepeatCheckoutItemResponse
{
    public string ProductId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}