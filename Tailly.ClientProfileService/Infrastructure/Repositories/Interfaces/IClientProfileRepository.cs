using Tailly.ClientProfileService.Core.Models;

namespace Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

public interface IClientProfileRepository
{
    Task AddAsync(ClientProfile profile);
    Task<ClientProfile?> GetByUserIdAsync(Guid userId);
    Task UpdateAsync(ClientProfile profile);
}