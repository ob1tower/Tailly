using Tailly.ShopService.Application.Dtos.Responses.Product;

namespace Tailly.ShopService.Application.Dtos.Responses.Catalog;

public sealed class CatalogProductsResponse
{
    public List<ProductResponse> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}