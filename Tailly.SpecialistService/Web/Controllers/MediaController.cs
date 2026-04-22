using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Application.Dtos.Requests.Media;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Specialist")]
[ApiController]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IValidator<UploadMediaRequest> _validator;

    public MediaController(IMediaService mediaService,
                           IValidator<UploadMediaRequest> validator)
    {
        _mediaService = mediaService;
        _validator = validator;
    }

    /// <summary>
    /// Uploads image for specialist avatar photo.
    /// </summary>
    /// <param name="file">Image file to upload.</param>
    /// <param name="mediaType">Type of media: avatar.</param>
    /// <returns>URL of the uploaded file.</returns>
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
