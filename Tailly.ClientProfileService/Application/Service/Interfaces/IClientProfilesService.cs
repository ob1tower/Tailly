using CSharpFunctionalExtensions;
using Tailly.ClientProfileService.Core.Common;
using Tailly.ClientProfileService.Core.Models;

namespace Tailly.ClientProfileService.Application.Service.Interfaces;

public interface IClientProfilesService
{
    Task<Result<ClientProfile, Error>> GetAsync(Guid userId);
    Task<Result> UpdateContactsAsync(Guid userId, ClientProfile profile);
    Task<Result> UpdateMainAsync(Guid userId, ClientProfile profile);
    Task<Result> CreateAsync(Guid userId, ClientProfile profile);
}