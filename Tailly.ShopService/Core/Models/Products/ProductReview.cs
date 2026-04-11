namespace Tailly.ShopService.Core.Models.Products;

public class ProductReview
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ProductReviewReply? Reply { get; set; }
}