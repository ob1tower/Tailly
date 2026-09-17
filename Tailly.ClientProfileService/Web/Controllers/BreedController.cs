using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ClientProfileService.Application.Dtos.Responses;
using Tailly.ClientProfileService.Application.Mappers;
using Tailly.ClientProfileService.Core.Models;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("breed")]
public class BreedController : ControllerBase
{
    private readonly IBreedRepository _repository;

    public BreedController(IBreedRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a list of all available pet breeds.
    /// </summary>
    /// <remarks>
    /// This endpoint returns a reference list of breeds used when creating or updating pets.
    /// </remarks>
    /// <returns>List of available breeds.</returns>
    [HttpGet("pets/breeds")]
    public async Task<IActionResult> GetAll()
    {
        var breeds = await _repository.GetAllAsync();

        var response = breeds.Select(x => new BreedResponse
        {
            Id = x.Id.ToString(),
            Type = PetMapper.MapPetType(x.Type)!,
            Title = x.Title
        });

        return Ok(response);
    }
}