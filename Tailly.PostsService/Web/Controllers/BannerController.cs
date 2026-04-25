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
}
