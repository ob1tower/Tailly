using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ShopService.Application.Dtos.Requests.Cart;
using Tailly.ShopService.Application.Dtos.Responses.Cart;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Infrastructure.Configurations.Extensions;

namespace Tailly.ShopService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    private readonly IValidator<AddToCartRequest> _addValidator;
    private readonly IValidator<UpdateCartItemRequest> _updateValidator;

    public CartController(ICartService cartService,
                          IValidator<AddToCartRequest> addValidator,
                          IValidator<UpdateCartItemRequest> updateValidator)
    {
        _cartService = cartService;
        _addValidator = addValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Gets the current cart for the authenticated user or guest session.
    /// </summary>
    /// <returns>Cart with items, total count and total price.</returns>
    [HttpGet("carts")]
    [EnableRateLimiting("cart")]
    public async Task<IActionResult> GetCart()
    {
        var (userId, sessionId) = HttpContext.ResolveIdentity();

        var result = await _cartService.GetCartAsync(userId, sessionId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var cart = result.Value;

        var response = new CartResponse
        {
            Items = cart.Items.Select(x => new CartItemResponse
            {
                ProductId = x.ProductId,
                ProductTitle = x.ProductTitle,
                ProductSlug = x.ProductSlug,
                ImageUrl = x.ImageUrl,
                Price = x.Price,
                OldPrice = x.OldPrice,
                Quantity = x.Quantity,
                LineTotal = x.LineTotal
            }).ToList(),
            TotalItems = cart.TotalItems,
            TotalPrice = cart.TotalPrice
        };

        return Ok(response);
    }

    /// <summary>
    /// Adds a product to the cart.
    /// </summary>
    /// <param name="productId">ID of the product to add.</param>
    /// <param name="request">Quantity of the product to add.</param>
    [HttpPost("carts/items/add/{productId:guid}")]
    [EnableRateLimiting("cart")]
    public async Task<IActionResult> AddItem(Guid productId, [FromBody] AddToCartRequest request)
    {
        var validation = await _addValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var (userId, sessionId) = HttpContext.ResolveIdentity();

        var result = await _cartService.AddItemAsync(userId, sessionId, productId, request.Quantity);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Merges guest cart with user cart after login.
    /// </summary>
    /// <param name="request">Merge option (true = merge, false = discard guest cart).</param>
    [HttpPost("carts/merge")]
    public async Task<IActionResult> Merge([FromBody] MergeRequest request)
    {
        var userId = User.GetUserId();
        var sessionId = HttpContext.GetSessionId();

        if (userId == null)
            return Unauthorized();

        await _cartService.HandleCartAfterLogin(userId.Value, sessionId, request.Merge);

        Response.Cookies.Delete("sessionId");

        return Ok();
    }

    /// <summary>
    /// Updates quantity of an item in the cart.
    /// </summary>
    /// <param name="productId">ID of the product to update.</param>
    /// <param name="request">New quantity for the cart item.</param>
    [HttpPut("carts/items/update/{productId:guid}")]
    [EnableRateLimiting("cart")]
    public async Task<IActionResult> UpdateItem(Guid productId, [FromBody] UpdateCartItemRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var (userId, sessionId) = HttpContext.ResolveIdentity();

        var result = await _cartService.UpdateItemAsync(userId, sessionId, productId, request.Quantity);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Removes a specific item from the cart.
    /// </summary>
    /// <param name="productId">ID of the product to remove.</param>
    [HttpDelete("carts/items/remove/{productId:guid}")]
    [EnableRateLimiting("cart")]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        var (userId, sessionId) = HttpContext.ResolveIdentity();

        var result = await _cartService.RemoveItemAsync(userId, sessionId, productId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    [HttpDelete("carts/clear")]
    [EnableRateLimiting("cart")]
    public async Task<IActionResult> ClearCart()
    {
        var (userId, sessionId) = HttpContext.ResolveIdentity();

        var result = await _cartService.ClearCartAsync(userId, sessionId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}