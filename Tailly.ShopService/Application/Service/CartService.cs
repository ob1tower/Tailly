using CSharpFunctionalExtensions;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Helpers;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Models.Cart;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Application.Service;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository cartRepository,
                       IProductRepository productRepository,
                       ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<Cart, Error>> GetCartAsync(Guid? userId, Guid? sessionId)
    {
        if (userId == null && sessionId == null)
            return ShopErrors.CartIdentityRequired;

        Cart? userCart = null;
        Cart? guestCart = null;

        if (userId != null)
            userCart = await _cartRepository.GetByUserIdAsync(userId.Value);

        if (sessionId != null)
            guestCart = await _cartRepository.GetBySessionIdAsync(sessionId.Value);

        Cart? cart = null;

        if (userId != null)
        {
            cart = userCart;
            if (cart == null)
            {
                cart = await _cartRepository.CreateAsync(userId, null);
            }
        }
        else
        {
            cart = guestCart;
            if (cart == null)
            {
                cart = await _cartRepository.CreateAsync(null, sessionId);
            }
        }

        cart = CartCalculator.Calculate(cart);

        return Result.Success<Cart, Error>(cart);
    }

    public async Task<Result> AddItemAsync(Guid? userId, Guid? sessionId, Guid productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("AddItem failed. Product not found: {ProductId}", productId);
            return Result.Failure(ShopErrors.ProductNotFound.Description);
        }

        var cartResult = await GetCartAsync(userId, sessionId);
        if (cartResult.IsFailure)
            return Result.Failure(cartResult.Error.Description);

        var cart = cartResult.Value;

        if (cart.Items.Any(x => x.ProductId == productId))
        {
            _logger.LogWarning("AddItem failed. Item already exists in cart: {ProductId}", productId);
            return Result.Failure(ShopErrors.ItemAlreadyExists.Description);
        }

        var item = new CartItem
        {
            ProductId = product.Id,
            ProductTitle = product.Title,
            ProductSlug = product.Slug,
            ImageUrl = product.Images.FirstOrDefault()?.Url,
            Price = product.Price,
            OldPrice = product.OldPrice,
            Quantity = quantity
        };

        await _cartRepository.AddItemAsync(cart.Id, item);

        _logger.LogInformation("Item successfully added to cart. ProductId: {ProductId}", productId);

        return Result.Success();
    }

    public async Task<Result> UpdateItemAsync(Guid? userId, Guid? sessionId, Guid productId, int quantity)
    {
        var cartResult = await GetCartAsync(userId, sessionId);
        if (cartResult.IsFailure)
            return Result.Failure(cartResult.Error.Description);

        var cart = cartResult.Value;

        if (!cart.Items.Any(x => x.ProductId == productId))
        {
            _logger.LogWarning("UpdateItem failed. Item not found: {ProductId}", productId);
            return Result.Failure(ShopErrors.ItemNotFound.Description);
        }

        await _cartRepository.UpdateItemAsync(cart.Id, productId, quantity);

        _logger.LogInformation("Item updated. ProductId: {ProductId}, New quantity: {Quantity}", productId, quantity);

        return Result.Success();
    }

    public async Task<Result> RemoveItemAsync(Guid? userId, Guid? sessionId, Guid productId)
    {
        var cartResult = await GetCartAsync(userId, sessionId);
        if (cartResult.IsFailure)
            return Result.Failure(cartResult.Error.Description);

        var cart = cartResult.Value;

        if (!cart.Items.Any(x => x.ProductId == productId))
        {
            _logger.LogWarning("RemoveItem failed. Item not found: {ProductId}", productId);
            return Result.Failure(ShopErrors.ItemNotFound.Description);
        }

        await _cartRepository.RemoveItemAsync(cart.Id, productId);

        _logger.LogInformation("Item removed from cart. ProductId: {ProductId}", productId);

        return Result.Success();
    }

    public async Task<Result> ClearCartAsync(Guid? userId, Guid? sessionId)
    {
        var cartResult = await GetCartAsync(userId, sessionId);
        if (cartResult.IsFailure)
            return Result.Failure(cartResult.Error.Description);

        await _cartRepository.ClearAsync(cartResult.Value.Id);

        _logger.LogInformation("Cart cleared successfully. CartId: {CartId}", cartResult.Value.Id);

        return Result.Success();
    }

    public async Task HandleCartAfterLogin(Guid userId, Guid? sessionId, bool merge)
    {
        if (sessionId == null)
            return;

        var guestCart = await _cartRepository.GetBySessionIdAsync(sessionId.Value);
        if (guestCart == null)
            return;

        if (merge)
        {
            var userCart = await _cartRepository.GetByUserIdAsync(userId);

            if (userCart == null)
            {
                await _cartRepository.AssignUserAsync(guestCart.Id, userId);
            }
            else
            {
                await _cartRepository.MergeAsync(userCart.Id, guestCart);
                await _cartRepository.DeleteBySessionIdAsync(sessionId.Value);
            }
        }
        else
        {
            await _cartRepository.DeleteBySessionIdAsync(sessionId.Value);
        }
    }
}