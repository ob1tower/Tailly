namespace Tailly.ShopService.Core.Models.Products;

public class ProductReviewReply
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}