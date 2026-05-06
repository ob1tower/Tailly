using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.BookingService.Application.Service.Interfaces;

namespace Tailly.BookingService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client, Specialist")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IValidator<IFormFile> _validator;
    private readonly IValidator<List<IFormFile>> _multipleValidator;

    public MediaController(IMediaService mediaService,
                           IValidator<IFormFile> validator,
                           IValidator<List<IFormFile>> multipleValidator)
    {
        _mediaService = mediaService;
        _validator = validator;
        _multipleValidator = multipleValidator;
    }

    /// <summary>
    /// Uploading a single image.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var validation = await _validator.ValidateAsync(file);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _mediaService.SaveAsync(file, "reviews");
        return Ok(result);
    }

    /// <summary>
    /// Uploading multiple images (maximum 10).
    /// </summary>
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultiple(List<IFormFile> files)
    {
        var validation = await _multipleValidator.ValidateAsync(files);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var results = await _mediaService.SaveMultipleAsync(files, "reviews");
        return Ok(results);
    }
}