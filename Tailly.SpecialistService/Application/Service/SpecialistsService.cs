using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Application.Service;

public class SpecialistsService : ISpecialistsService
{
    private readonly ISpecialistRepository _repository;
    private readonly ILogger<SpecialistsService> _logger;

    public SpecialistsService(ISpecialistRepository repository,
                              ILogger<SpecialistsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Specialist>> SearchAsync(string? cityQuery, string? districtQuery,
           string? serviceType, decimal? priceMin, decimal? priceMax, int page, int pageSize)
    {
        _logger.LogInformation("Searching specialists. City: {City}, District: {District}, ServiceType: {ServiceType}, Page: {Page}",
            cityQuery, districtQuery, serviceType, page);

        return await _repository.SearchAsync(cityQuery, districtQuery, serviceType, priceMin, priceMax, page, pageSize);
    }

    public async Task<Specialist?> GetFullProfileByIdAsync(Guid id)
    {
        return await _repository.GetFullProfileByIdAsync(id);
    }

    public async Task<Specialist?> GetFullProfileBySlugAsync(string slug)
    {
        return await _repository.GetFullProfileBySlugAsync(slug);
    }
}