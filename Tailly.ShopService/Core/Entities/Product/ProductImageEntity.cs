namespace Tailly.ShopService.Core.Entities.Product;

public class ProductImageEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}