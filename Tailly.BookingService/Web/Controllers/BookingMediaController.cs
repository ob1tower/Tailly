using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.BookingService.Application.Dtos.Requests;
using Tailly.BookingService.Application.Errors;
using Tailly.BookingService.Application.Service.Interfaces;
using Tailly.BookingService.Infrastructure.Configurations.Extensions;

namespace Tailly.BookingService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client")]
public class BookingMediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IValidator<UploadMediaRequest> _validator;

    public BookingMediaController(IMediaService mediaService,
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