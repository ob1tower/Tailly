using Tailly.BookingService.Core.Models;

namespace Tailly.BookingService.Infrastructure.Repositories
{
    public interface IServiceOrderRepository
    {
        Task AddAsync(ServiceOrder order);
        Task<bool> ExistsAsync(Guid id);
        Task UpdateAsync(ServiceOrder order);
        Task<List<ServiceOrder>> GetByClientIdAsync(Guid clientId, string? statusFilter = null, int page = 1, int limit = 20);
        Task<ServiceOrder?> GetByIdAsync(Guid id);
        Task<List<ServiceOrder>> GetBySpecialistIdAsync(Guid specialistId, string? statusFilter = null, int page = 1, int limit = 20);
    }
}