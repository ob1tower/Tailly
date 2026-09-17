using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Applications;

namespace Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

public interface ISpecialistApplicationRepository
{
    Task AddAsync(SpecialistApplication application);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> HasPendingOrApprovedApplicationAsync(string email);
    Task<(List<SpecialistApplication> items, int total)> GetAllAsync(int page, int limit, SpecialistApplicationStatus? status = null);
    Task<SpecialistApplication?> GetByIdAsync(Guid id);
    Task UpdateAsync(SpecialistApplication application);
    Task<bool> HasInterviewConflictAsync(string reviewedBy, DateTime interviewDate);
}