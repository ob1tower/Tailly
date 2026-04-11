using Tailly.ShopService.Application.Dtos.Responses.Product;

namespace Tailly.ShopService.Application.Dtos.Responses.Order;

public sealed class OrderItemResponse
{
    public string ProductId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public List<ProductImageResponse> Images { get; set; } = [];
}