namespace Tailly.ShopService.Core.Entities.Product;

public class ProductReviewImageEntity
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public ProductReviewEntity Review { get; set; } = null!;
    public string Url { get; set; } = string.Empty;
}