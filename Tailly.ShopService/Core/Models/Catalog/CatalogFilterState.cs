using Tailly.ShopService.Core.Enums;

namespace Tailly.ShopService.Core.Models.Catalog;

public class CatalogFilterState
{
    public string? Search { get; set; } = null;
    public List<string> CategoryIds { get; set; } = [];
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool OnlyAvailable { get; set; }
    public ProductSort Sort { get; set; } = ProductSort.Newest;
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}