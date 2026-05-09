using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.PostsService.Application.Service.Interfaces;

namespace Tailly.PostsService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PostMediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public PostMediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    /// <summary>
    /// Uploads image for post and banner.
    /// </summary>
    /// <param name="file">Image file</param>   
    /// <returns>URL of uploaded image</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null)
            return BadRequest("File is required.");

        var url = await _mediaService.SaveAsync(file, "posts and banners");

        return Ok(new { url });
    }
}