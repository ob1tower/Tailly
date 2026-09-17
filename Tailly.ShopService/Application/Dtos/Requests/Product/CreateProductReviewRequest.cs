namespace Tailly.ShopService.Application.Dtos.Requests.Product;

public sealed class CreateProductReviewRequest
{ 
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Images { get; set; } = [];
}