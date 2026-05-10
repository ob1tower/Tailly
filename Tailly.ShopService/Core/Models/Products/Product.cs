namespace Tailly.ShopService.Core.Models.Products;

public class Product
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
    public string? Brand { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? ForWhom { get; set; }
    public string? Purpose { get; set; }
    public string? PetSize { get; set; }
    public string? Material { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductImage> Images { get; set; } = [];
    public List<ProductReview> Reviews { get; set; } = [];
}