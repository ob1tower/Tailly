using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.PostsService.Application.Service.Interfaces;
using Tailly.PostsService.Infrastructure.Configurations.Extensions;

namespace Tailly.PostsService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminContentController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IBannerService _bannerService;

    public AdminContentController(IPostService postService, 
                                  IBannerService bannerService)
    {
        _postService = postService;
        _bannerService = bannerService;
    }

    /// <summary>
    /// Returns meta data for admin content page (tags + total pages for posts and banners).
    /// </summary>
    [HttpGet("admin/content/meta")]
    public async Task<IActionResult> GetMeta()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var postsResult = await _postService.GetListAsync(
            page: 1,
            limit: 1,
            search: null,
            tag: null,
            sort: null);

        if (postsResult.IsFailure)
            return BadRequest(postsResult.Error);

        var (_, postsTotal, tags) = postsResult.Value;
        var postTotalPages = (int)Math.Ceiling(postsTotal / 10.0);

        var bannersResult = await _bannerService.GetListAsync(1, 1, null, null);

        int bannerTotalPages = 0;
        if (bannersResult.IsSuccess)
        {
            var (_, bannersTotal) = bannersResult.Value;
            bannerTotalPages = (int)Math.Ceiling(bannersTotal / 10.0);
        }

        return Ok(new
        {
            postTagOptions = tags,
            postsTotalPages = postTotalPages,
            bannersTotalPages = bannerTotalPages
        });
    }
}