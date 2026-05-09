using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ClientProfileService.Application.Dtos.Requests;
using Tailly.ClientProfileService.Application.Errors;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.ClientProfileService.Infrastructure.Configurations.Extensions;

namespace Tailly.ClientProfileService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class ClientMediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IValidator<UploadMediaRequest> _validator;

    public ClientMediaController(IMediaService mediaService,
                           IValidator<UploadMediaRequest> validator)
    {
        _mediaService = mediaService;
        _validator = validator;
    }

    /// <summary>
    /// Uploads image for client avatar or pet photo.
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="mediaType">Type of media: avatar or pet</param>
    /// <returns>URL of the uploaded file</returns>
    [EnableRateLimiting("media")]
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
        {
            return BadRequest(result.Error);
        }

        return Ok(new { url = result.Value });
    }
}