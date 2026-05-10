namespace Tailly.ShopService.Application.Dtos.Responses.Product;

public sealed class ProductResponse
{
    public string Id { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string CategoryId { get; set; } = default!;
    public string CategoryTitle { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
    public ProductCharacteristicsResponse Characteristics { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductImageResponse> Images { get; set; } = [];
    public List<ProductReviewResponse> Reviews { get; set; } = [];
}