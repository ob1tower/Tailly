using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Infrastructure.Repositories.Interfaces
{
    public interface IAdminPasswordRecoveryRepository
    {
        Task AddAsync(AdminPasswordRecovery model);
        Task<List<AdminPasswordRecovery>> GetAllAsync();
        Task<AdminPasswordRecovery?> GetByIdAsync(Guid id);
        Task UpdateAsync(AdminPasswordRecovery model);
    }
}