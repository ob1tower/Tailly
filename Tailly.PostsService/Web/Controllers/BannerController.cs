using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.PostsService.Application.Mappers;
using Tailly.PostsService.Application.Service.Interfaces;

namespace Tailly.PostsService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BannerController : ControllerBase
{
    private readonly IBannerService _bannerService;

    public BannerController(IBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    /// <summary>
    /// Gets all currently active banners.
    /// </summary>
    [HttpGet("banners")]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetActiveBanners()
    {
        var result = await _bannerService.GetActiveBannersAsync();

        if (result.IsFailure)
            return BadRequest(result.Error);

        var response = result.Value.Select(BannerMapper.ToResponse);
        return Ok(response);
    }

    /// <summary>
    /// Gets banners by placement.
    /// </summary>
    /// <param name="placement">Banner placement: home_hero, posts, specialists, shop.</param>
    /// <returns>List of active banners.</returns>
    [HttpGet("banners/{placement}")]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> GetBannersByPlacement(string placement)
    {
        var result = await _bannerService.GetBannersByPlacementAsync(placement);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var response = result.Value.Select(BannerMapper.ToResponse);
        return Ok(response);
    }
}
