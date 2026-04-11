namespace Tailly.ShopService.Application.Dtos.Responses.Product;

public sealed class ProductImageResponse
{
    public string Id { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string Alt { get; set; } = default!;
}