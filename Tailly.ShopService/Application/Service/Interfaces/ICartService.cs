using CSharpFunctionalExtensions;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Models.Cart;

namespace Tailly.ShopService.Application.Service.Interfaces;

public interface ICartService
{
    Task<Result> AddItemAsync(Guid? userId, Guid? sessionId, Guid productId, int quantity);
    Task<Result> ClearCartAsync(Guid? userId, Guid? sessionId);
    Task<Result<Cart, Error>> GetCartAsync(Guid? userId, Guid? sessionId);
    Task<Result> RemoveItemAsync(Guid? userId, Guid? sessionId, Guid productId);
    Task<Result> UpdateItemAsync(Guid? userId, Guid? sessionId, Guid productId, int quantity);
    Task HandleCartAfterLogin(Guid userId, Guid? sessionId, bool merge);
}