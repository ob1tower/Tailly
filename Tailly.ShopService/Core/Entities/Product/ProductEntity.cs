namespace Tailly.ShopService.Core.Entities.Product;

public class ProductEntity
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public CategoryEntity? Category { get; set; }
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ProductReviewEntity> Reviews { get; set; } = [];
    public ICollection<ProductImageEntity> Images { get; set; } = [];
}