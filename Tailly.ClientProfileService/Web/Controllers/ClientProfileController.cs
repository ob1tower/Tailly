using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Tailly.ClientProfileService.Application.Dtos.Requests;
using Tailly.ClientProfileService.Application.Errors;
using Tailly.ClientProfileService.Application.Mappers;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.ClientProfileService.Infrastructure.Configurations.Extensions;

namespace Tailly.ClientProfileService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
[EnableRateLimiting("profile")]
public class ClientProfileController : ControllerBase
{
    private readonly IClientProfilesService _service;
    private readonly IValidator<UpdateClientProfileMainRequest> _mainValidator;
    private readonly IValidator<UpdateClientProfileContactsRequest> _contactsValidator;

    public ClientProfileController(IClientProfilesService service,
                                   IValidator<UpdateClientProfileMainRequest> mainValidator,
                                   IValidator<UpdateClientProfileContactsRequest> contactsValidator)
    {
        _service = service;
        _mainValidator = mainValidator;
        _contactsValidator = contactsValidator;
    }

    /// <summary>
    /// Retrieves the current client's profile.
    /// </summary>
    /// <remarks>
    /// Returns profile information for the authenticated user.
    /// </remarks>
    /// <returns>Client profile data.</returns>
    [HttpGet("me")]
    public async Task<IActionResult> Get()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value;

        var result = await _service.GetAsync(userId.Value);

        if (result.IsFailure)
        {
            return result.Error == ClientProfileErrors.ProfileNotFound
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(ClientProfileDtoMapper.ToResponse(result.Value, email));
    }

    /// <summary>
    /// Updates main client profile information.
    /// </summary>
    /// <param name="request">Main profile data to update.</param>
    /// <returns>Updated client profile.</returns>
    [HttpPut("me/main")]
    public async Task<IActionResult> UpdateMain([FromBody] UpdateClientProfileMainRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var validation = await _mainValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var model = ClientProfileDtoMapper.ToModel(request);

        var result = await _service.UpdateMainAsync(userId.Value, model);

        if (result.IsFailure)
        {
            return result.Error == ClientProfileErrors.ProfileNotFound.Description
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        var updated = await _service.GetAsync(userId.Value);

        if (updated.IsFailure)
            return NotFound(updated.Error);

        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value;

        return Ok(ClientProfileDtoMapper.ToResponse(updated.Value, email));
    }

    /// <summary>
    /// Updates client contact information.
    /// </summary>
    /// <param name="request">Contact details to update.</param>
    /// <returns>Updated client profile.</returns>
    [HttpPut("me/contacts")]
    public async Task<IActionResult> UpdateContacts([FromBody] UpdateClientProfileContactsRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var validation = await _contactsValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var model = ClientProfileDtoMapper.ToModel(request);

        var result = await _service.UpdateContactsAsync(userId.Value, model);

        if (result.IsFailure)
        {
            return result.Error == ClientProfileErrors.ProfileNotFound.Description
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        var updated = await _service.GetAsync(userId.Value);

        if (updated.IsFailure)
            return NotFound(updated.Error);

        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value;

        return Ok(ClientProfileDtoMapper.ToResponse(updated.Value, email));
    }
}