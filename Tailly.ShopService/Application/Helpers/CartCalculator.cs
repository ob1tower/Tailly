using Tailly.ShopService.Core.Models.Cart;

namespace Tailly.ShopService.Application.Helpers;

public static class CartCalculator
{
    public static Cart Calculate(Cart cart)
    {
        var totalPrice = cart.Items.Sum(x => x.LineTotal);
        var totalItems = cart.Items.Sum(x => x.Quantity);

        cart.TotalPrice = totalPrice;
        cart.TotalItems = totalItems;

        return cart;
    }
}