namespace Tailly.ShopService.Core.Entities.Product;

public class CategoryEntity
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ICollection<ProductEntity> Products { get; set; } = [];
}