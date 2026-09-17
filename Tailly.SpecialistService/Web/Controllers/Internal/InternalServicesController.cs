using Microsoft.AspNetCore.Mvc;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Web.Controllers.Internal;

[ApiController]
[Route("internal/services")]
[ApiExplorerSettings(IgnoreApi = true)]
public class InternalServicesController : ControllerBase
{
    private readonly ISpecialistRepository _repository;

    public InternalServicesController(ISpecialistRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{serviceId:guid}")]
    public async Task<IActionResult> GetById(Guid serviceId)
    {
        var service = await _repository.GetServiceByIdAsync(serviceId);

        if (service == null)
            return NotFound();

        return Ok(new
        {
            service.Id,
            service.SpecialistId,
            Name = service.Name.ToString(),
            service.Price,
            PriceUnit = service.PriceUnit.ToString()
        });
    }
}