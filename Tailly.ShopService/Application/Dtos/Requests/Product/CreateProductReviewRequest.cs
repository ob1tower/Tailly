namespace Tailly.ShopService.Application.Dtos.Requests.Product;

public sealed class CreateProductReviewRequest
{
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }      
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
}