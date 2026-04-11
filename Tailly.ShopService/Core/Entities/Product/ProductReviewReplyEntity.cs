namespace Tailly.ShopService.Core.Entities.Product;

public class ProductReviewReplyEntity
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public ProductReviewEntity? Review { get; set; }
    public string AuthorName { get; set; } = "Tailly Shop";
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}