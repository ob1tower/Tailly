namespace Tailly.ShopService.Application.Dtos.Responses.Product;

public sealed class ProductReviewResponse
{
    public string Id { get; set; } = default!;
    public string AuthorName { get; set; } = default!;
    public int Rating { get; set; }
    public string Text { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public ProductReviewReplyResponse? siteReply { get; set; } // ВРЕМЕННО
    public List<ProductReviewImageResponse> Images { get; set; } = [];
}