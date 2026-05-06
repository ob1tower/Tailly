using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Infrastructure.Repositories.Interfaces
{
    public interface IAdminProfileRepository
    {
        Task AddAsync(AdminProfileEntity entity);
        Task<AdminProfileEntity?> GetByUserIdAsync(Guid userId);
        Task DeleteAsync(Guid profileId);
        Task UpdateAsync(AdminProfileEntity entity);
    }
}