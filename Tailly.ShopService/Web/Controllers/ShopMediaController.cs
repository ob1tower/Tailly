using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.ShopService.Application.Dtos.Requests.Media;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Infrastructure.Configurations.Extensions;

namespace Tailly.ShopService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client,Specialist")]
public class ShopMediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IValidator<UploadMediaRequest> _validator;

    public ShopMediaController(IMediaService mediaService,
                           IValidator<UploadMediaRequest> validator)
    {
        _mediaService = mediaService;
        _validator = validator;
    }

    /// <summary>
    /// Upload review image.
    /// </summary>
    /// <param name="file">Image file.</param>
    /// <param name="mediaType">Media type: reviews.</param>
    /// <returns>Uploaded file URL.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string mediaType)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var request = new UploadMediaRequest
        {
            File = file,
            MediaType = mediaType ?? string.Empty
        };

        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _mediaService.UploadAsync(file, mediaType, userId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { url = result.Value });
    }
}