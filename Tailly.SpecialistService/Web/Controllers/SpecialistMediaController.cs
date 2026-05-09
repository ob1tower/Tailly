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
public class SpecialistMediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IValidator<UploadMediaRequest> _validator;

    public SpecialistMediaController(IMediaService mediaService,
                           IValidator<UploadMediaRequest> validator)
    {
        _mediaService = mediaService;
        _validator = validator;
    }

    /// <summary>
    /// Uploading an image (avatar or photo to the gallery).
    /// </summary>
    /// <param name="file">Image file to upload.</param>
    /// <param name="mediaType">Type of media: avatar, specialist_gallery.</param>
    /// <returns>URL of the uploaded file.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string mediaType)
    {
        var specialistId = User.GetSpecialistId();
        if (specialistId == null)
            return Unauthorized();

        var request = new UploadMediaRequest
        {
            File = file,
            MediaType = mediaType ?? string.Empty
        };

        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var result = await _mediaService.UploadAsync(file, mediaType, specialistId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { url = result.Value });
    }
}
