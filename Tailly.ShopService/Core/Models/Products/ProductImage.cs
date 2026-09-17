namespace Tailly.ShopService.Core.Models.Products;

public class ProductImage
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}