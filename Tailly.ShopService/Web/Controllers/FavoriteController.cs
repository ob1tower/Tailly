using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ShopService.Application.Dtos.Responses.User;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Infrastructure.Configurations.Extensions;

namespace Tailly.ShopService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoriteController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>
    /// Gets the current user's or guest's favorites list.
    /// </summary>
    /// <returns>List of favorite items with product information and "in cart" flag.</returns>
    [HttpGet("favorites")]
    [EnableRateLimiting("favorites")]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = User.GetUserId();
        var sessionId = HttpContext.GetOrCreateSessionId();

        var result = await _favoriteService.GetAsync(userId, sessionId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var favorites = result.Value;

        var response = new FavoriteListResponse
        {
            Items = favorites.Select(x => new FavoriteItemResponse
            {
                ProductId = x.ProductId,
                Title = x.Title,
                Slug = x.Slug,
                Price = x.Price,
                OldPrice = x.OldPrice,
                ImageUrl = x.ImageUrl,
                InCart = x.InCart
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Adds a product to favorites (for authorized user or guest).
    /// </summary>
    /// <param name="productId">ID of the product to add to favorites.</param>
    [HttpPost("favorites/{productId:guid}")]
    [EnableRateLimiting("favorites")]
    public async Task<IActionResult> Add(Guid productId)
    {
        var userId = User.GetUserId();
        var sessionId = HttpContext.GetOrCreateSessionId();

        var result = await _favoriteService.AddAsync(userId, sessionId, productId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Removes a product from favorites.
    /// </summary>
    /// <param name="productId">ID of the product to remove from favorites.</param>
    [HttpDelete("favorites/{productId:guid}")]
    public async Task<IActionResult> Remove(Guid productId)
    {
        var userId = User.GetUserId();
        var sessionId = HttpContext.GetOrCreateSessionId();

        var result = await _favoriteService.RemoveAsync(userId, sessionId, productId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    /// <summary>
    /// Clears all favorites for the current user or guest session.
    /// </summary>
    [HttpDelete("favorites/clear")]
    public async Task<IActionResult> Clear()
    {
        var userId = User.GetUserId();
        var sessionId = HttpContext.GetOrCreateSessionId();

        var result = await _favoriteService.ClearAsync(userId, sessionId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}
