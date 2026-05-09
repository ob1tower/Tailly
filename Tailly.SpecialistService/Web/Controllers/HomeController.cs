using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Infrastructure.Configurations.Options;

namespace Tailly.SpecialistService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly ISpecialistsService _service;
    private readonly ApiSettings _apiSettings;

    public HomeController(ISpecialistsService service,
                          IOptions<ApiSettings> apiSettings)
    {
        _service = service;
        _apiSettings = apiSettings.Value;
    }

    /// <summary>
    /// Get a list of services for the main page.
    /// </summary>
    /// <returns>List of popular services.</returns>
    [HttpGet("home/services")]
    public IActionResult GetServices()
    {
        var baseUrl = _apiSettings.BaseUrl;

        var services = new[]
        {
            new
            {
                id = "walking",
                title = "Выгул",
                subtitle = "Индивидуальный подход,\r\nактивные прогулки и полная\r\nбезопасность",
                iconUrl = $"{baseUrl}/service-icons/walking.svg"
            },
            new
            {
                id = "boarding",
                title = "Передержка",
                subtitle = "Круглосуточный присмотр,\r\nсбалансированное питание\r\nи любовь к вашему питомцу",
                iconUrl = $"{baseUrl}/service-icons/boarding.svg"
            },
            new
            {
                id = "grooming",
                title = "Груминг",
                subtitle = "Красивые стрижки, гигиена\r\nи спа-уход",
                iconUrl = $"{baseUrl}/service-icons/grooming.svg"
            },
            new
            {
                id = "training",
                title = "Тренировки",
                subtitle = "Коррекция поведения,освоение\r\nкоманд и социализация\r\nпод руководством зоопсихолога",
                iconUrl = $"{baseUrl}/service-icons/training.svg"
            },
            new
            {
                id = "photoshoot",
                title = "Фотосессия",
                subtitle = "Сохраним самые трогательные\r\nмоменты в идеальном качестве",
                iconUrl = $"{baseUrl}/service-icons/photoshoot.svg"
            }
        };

        return Ok(services);
    }

    /// <summary>
    /// Get reviews to display on the home page.
    /// </summary>
    /// <param name="rating">Minimum review rating.</param>
    /// <param name="limit">Number of reviews.</param>
    /// <param name="requirePhotos">Availability of photos.</param>
    /// <param name="minTextLength">Minimum text length.</param>
    /// <param name="minWords">Minimum number of words.</param>
    /// <returns>List of reviews.</returns>
    [HttpGet("home/reviews")]
    public async Task<IActionResult> GetHomeReviews([FromQuery] int rating = 5, [FromQuery] int limit = 5, [FromQuery] bool requirePhotos = true, [FromQuery] int minTextLength = 80, [FromQuery] int minWords = 8)
    {
        var reviews = await _service.GetHomeReviewsAsync(
            rating,
            limit,
            requirePhotos,
            minTextLength,
            minWords);

        return Ok(reviews);
    }
}