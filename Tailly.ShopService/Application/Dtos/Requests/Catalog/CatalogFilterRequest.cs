namespace Tailly.ShopService.Application.Dtos.Requests.Catalog;

public sealed class CatalogFilterRequest
{
    public string? Search { get; set; }
    public List<string> CategoryIds { get; set; } = new();
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool OnlyAvailable { get; set; } = false;
    public string Sort { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}