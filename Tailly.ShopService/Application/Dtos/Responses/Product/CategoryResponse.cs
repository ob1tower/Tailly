namespace Tailly.ShopService.Application.Dtos.Responses.Product;

public sealed class CategoryResponse
{
    public string Id { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
}