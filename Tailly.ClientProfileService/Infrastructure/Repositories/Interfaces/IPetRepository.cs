using Tailly.ClientProfileService.Core.Models;

namespace Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

public interface IPetRepository
{
    Task AddAsync(Pet pet);
    Task DeleteAsync(Guid id);
    Task<List<Pet>> GetByClientIdAsync(Guid clientId);
    Task<Pet?> GetByIdAsync(Guid id);
    Task UpdateAsync(Pet pet);
}