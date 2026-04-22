using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

public interface ISpecialistRepository
{
    Task AddAsync(Specialist specialist);
    Task<Specialist?> GetBySlugAsync(string slug);
    Task<Specialist?> GetByIdAsync(Guid id);
    Task<bool> SlugExistsAsync(string slug);
    Task UpdateAsync(Specialist specialist);
    Task<bool> ExistsByEmailAsync(string email);
    Task<(List<Specialist> specialists, int total)> GetAllAsync(string? cityQuery, string? districtQuery, Guid? serviceId, decimal? priceMin, decimal? priceMax,
                                                                              int? experienceMinYears, bool hasReviewsOnly, SpecialistSort sort, int page, int limit);
}