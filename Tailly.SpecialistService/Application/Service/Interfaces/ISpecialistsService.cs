using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Application.Service.Interfaces;

public interface ISpecialistsService
{
    Task<List<Specialist>> SearchAsync(string? cityQuery, string? districtQuery, string? serviceType, decimal? priceMin, decimal? priceMax, int page, int pageSize);
    Task<Specialist?> GetFullProfileByIdAsync(Guid id);
    Task<Specialist?> GetFullProfileBySlugAsync(string slug);
}