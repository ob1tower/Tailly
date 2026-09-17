using Tailly.ShopService.Application.Dtos.Responses.Product;

namespace Tailly.ShopService.Application.Dtos.Responses.Catalog;

public sealed class CatalogMetaResponse
{
    public List<CategoryResponse> Categories { get; set; } = [];
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public List<string> AvailableSorts { get; set; } = [];
}