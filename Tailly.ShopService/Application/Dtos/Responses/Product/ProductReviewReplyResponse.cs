namespace Tailly.ShopService.Application.Dtos.Responses.Product;

public sealed class ProductReviewReplyResponse
{
    public string AuthorName { get; set; } = default!;
    public string Text { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}