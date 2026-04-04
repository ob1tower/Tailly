using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ClientProfileService.Application.Dtos.Requests;
using Tailly.ClientProfileService.Application.Errors;
using Tailly.ClientProfileService.Application.Mappers;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.ClientProfileService.Infrastructure.Configurations.Extensions;

namespace Tailly.ClientProfileService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
[EnableRateLimiting("pet")]
public class PetController : ControllerBase
{
    private readonly IPetService _service;
    private readonly IValidator<UpsertPetRequest> _validator;

    public PetController(IPetService service,
                         IValidator<UpsertPetRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>
    /// Retrieves all pets for the current user.
    /// </summary>
    /// <returns>List of user's pets.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _service.GetByUserAsync(userId.Value);

        if (result.IsFailure)
        {
            return result.Error == ClientProfileErrors.ProfileNotFound
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(result.Value.Select(PetDtoMapper.ToResponse));
    }

    /// <summary>
    /// Creates a new pet for the current user.
    /// </summary>
    /// <param name="request">Pet data.</param>
    /// <returns>Created pet.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertPetRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var pet = PetDtoMapper.ToModel(request);

        var result = await _service.CreateAsync(userId.Value, pet);

        if (result.IsFailure)
        {
            return result.Error == ClientProfileErrors.ProfileNotFound
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(PetDtoMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Updates an existing pet.
    /// </summary>
    /// <param name="id">Pet identifier.</param>
    /// <param name="request">Updated pet data.</param>
    /// <returns>Operation result.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertPetRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var pet = PetDtoMapper.ToModel(request);
        pet.Id = id;

        var result = await _service.UpdateAsync(userId.Value, pet);

        if (result.IsFailure)
        {
            if (result.Error == ClientProfileErrors.PetNotFound.Description)
                return NotFound(result.Error);

            if (result.Error == ClientProfileErrors.AccessDenied.Description)
                return Forbid();

            return BadRequest(result.Error);
        }

        return Ok();
    }

    /// <summary>
    /// Deletes a pet.
    /// </summary>
    /// <param name="id">Pet identifier.</param>
    /// <returns>Operation result.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _service.DeleteAsync(userId.Value, id);

        if (result.IsFailure)
        {
            if (result.Error == ClientProfileErrors.PetNotFound.Description)
                return NotFound(result.Error);

            if (result.Error == ClientProfileErrors.AccessDenied.Description)
                return Forbid();

            return BadRequest(result.Error);
        }

        return Ok();
    }
}