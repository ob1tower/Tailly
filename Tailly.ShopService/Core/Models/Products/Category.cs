namespace Tailly.ShopService.Core.Models.Products;

public class Category
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}