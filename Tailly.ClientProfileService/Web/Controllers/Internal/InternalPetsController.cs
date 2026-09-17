using Microsoft.AspNetCore.Mvc;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Web.Controllers.Internal;

[ApiController]
[Route("internal/pets")]
[ApiExplorerSettings(IgnoreApi = true)]
public class InternalPetsController : ControllerBase
{
    private readonly IPetRepository _repository;

    public InternalPetsController(IPetRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{petId:guid}")]
    public async Task<IActionResult> GetById(Guid petId)
    {
        var pet = await _repository.GetEntityByIdAsync(petId);

        if (pet == null)
            return NotFound();

        return Ok(new
        {
            pet.Id,
            UserId = pet.ClientProfile.UserId,
            pet.Name
        });
    }
}