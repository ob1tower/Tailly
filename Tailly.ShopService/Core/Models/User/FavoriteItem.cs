namespace Tailly.ShopService.Core.Models.User;

public class FavoriteItem
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool InCart { get; set; }
}