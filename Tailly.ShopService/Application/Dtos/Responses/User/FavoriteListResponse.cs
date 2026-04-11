namespace Tailly.ShopService.Application.Dtos.Responses.User;

public sealed class FavoriteListResponse
{
    public List<FavoriteItemResponse> Items { get; set; } = [];
}