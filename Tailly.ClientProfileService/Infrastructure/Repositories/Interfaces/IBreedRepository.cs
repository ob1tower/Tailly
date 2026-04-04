using Tailly.ClientProfileService.Core.Models;

namespace Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

public interface IBreedRepository
{
    Task<List<Breed>> GetAllAsync();
}