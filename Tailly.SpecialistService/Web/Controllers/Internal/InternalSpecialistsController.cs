using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Web.Controllers.Internal;

[ApiController]
[Route("internal/specialists")]
[ApiExplorerSettings(IgnoreApi = true)]
public class InternalSpecialistsController : ControllerBase
{
    private readonly ISpecialistRepository _repository;

    public InternalSpecialistsController(ISpecialistRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{specialistId:guid}")]
    public async Task<IActionResult> GetById(Guid specialistId)
    {
        var specialist = await _repository.GetByIdAsync(specialistId);

        if (specialist == null)
            return NotFound();

        return Ok(new
        {
            specialist.Id,
            specialist.Slug,
            FullName = $"{specialist.FirstName} {specialist.LastName}"
        });
    }
}