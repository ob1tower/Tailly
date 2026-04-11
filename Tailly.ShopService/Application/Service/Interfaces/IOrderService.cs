using CSharpFunctionalExtensions;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Models.Cart;
using Tailly.ShopService.Core.Models.Order;
using Tailly.ShopService.Core.Models.Order.Checkout;

namespace Tailly.ShopService.Application.Service.Interfaces;

public interface IOrderService
{
    Task<Result> CancelOrderAsync(Guid userId, Guid orderId);
    Task<Result> ConfirmPaymentAsync(Guid userId, Guid orderId);
    Task<Result<Order, Error>> CreateOrderAsync(Guid userId, CheckoutForm form, List<CartItem> cartItems);
    Task<Result<Order, Error>> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task<Result<List<Order>, Error>> GetUserOrdersAsync(Guid userId);
    Task<Result<OrderRepeatCheckoutDraft, Error>> RepeatOrderAsync(Guid userId, Guid orderId);
    Task<Result<string, Error>> StartPaymentAsync(Guid userId, Guid orderId);
}