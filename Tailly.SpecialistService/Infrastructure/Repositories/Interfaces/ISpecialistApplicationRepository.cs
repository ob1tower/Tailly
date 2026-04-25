using Tailly.SpecialistService.Core.Models.Applications;

namespace Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

public interface ISpecialistApplicationRepository
{
    Task AddAsync(SpecialistApplication application);
    Task<bool> ExistsAsync(Guid id);
    Task<(List<SpecialistApplication> items, int total)> GetAllAsync(int page, int limit);
    Task<SpecialistApplication?> GetByIdAsync(Guid id);
    Task UpdateAsync(SpecialistApplication application);
    Task<bool> HasActiveApplicationAsync(string email);
}