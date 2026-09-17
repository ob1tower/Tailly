namespace Tailly.ShopService.Core.Models.User;

public class Favorite
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime AddedAt { get; set; }
}