namespace Tailly.ShopService.Core.Entities.Product;

public class ProductReviewEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }
    public Guid UserId { get; set; }               
    public Guid OrderId { get; set; }             
    public string AuthorName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ProductReviewImageEntity> Images { get; set; } = [];
    public Guid? ReplyId { get; set; }
    public ProductReviewReplyEntity? Reply { get; set; }
}