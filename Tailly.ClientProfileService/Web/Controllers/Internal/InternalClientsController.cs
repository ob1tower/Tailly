using Microsoft.AspNetCore.Mvc;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Web.Controllers.Internal;

[ApiController]
[Route("internal/clients")]
[ApiExplorerSettings(IgnoreApi = true)]
public class InternalClientsController : ControllerBase
{
    private readonly IClientProfileRepository _repository;

    public InternalClientsController(IClientProfileRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);

        if (profile == null)
            return NotFound();

        return Ok(new
        {
            profile.UserId,
            FullName = $"{profile.FirstName} {profile.LastName}"
        });
    }
}