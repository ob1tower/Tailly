using CSharpFunctionalExtensions;
using Tailly.ClientProfileService.Core.Common;
using Tailly.ClientProfileService.Core.Models;

namespace Tailly.ClientProfileService.Application.Service.Interfaces;

public interface IPetService
{
    Task<Result<Pet, Error>> CreateAsync(Guid userId, Pet pet);
    Task<Result> DeleteAsync(Guid userId, Guid petId);
    Task<Result<Pet, Error>> GetByIdAsync(Guid userId, Guid petId);
    Task<Result<List<Pet>, Error>> GetByUserAsync(Guid userId);
    Task<Result> UpdateAsync(Guid userId, Pet pet);
}