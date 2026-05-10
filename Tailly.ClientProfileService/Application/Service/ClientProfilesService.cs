using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.ClientProfileService.Application.Errors;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.ClientProfileService.Core.Common;
using Tailly.ClientProfileService.Core.Models;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;
using Tailly.Contracts.Messages;

namespace Tailly.ClientProfileService.Application.Service;

public class ClientProfilesService : IClientProfilesService
{
    private readonly IClientProfileRepository _repository;
    private readonly ILogger<ClientProfilesService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public ClientProfilesService(IClientProfileRepository repository,
                                 ILogger<ClientProfilesService> logger,
                                 IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<ClientProfile, Error>> GetAsync(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            _logger.LogWarning("Profile not found for user {UserId}", userId);
            return Result.Failure<ClientProfile, Error>(ClientProfileErrors.ProfileNotFound);
        }

        return Result.Success<ClientProfile, Error>(profile);
    }

    public async Task<Result> UpdateMainAsync(Guid userId, ClientProfile profile)
    {
        var existingResult = await GetAsync(userId);
        if (existingResult.IsFailure)
            return Result.Failure(existingResult.Error.Description);

        var existing = existingResult.Value;

        if (!string.IsNullOrWhiteSpace(profile.FirstName))
            existing.FirstName = profile.FirstName;

        if (!string.IsNullOrWhiteSpace(profile.LastName))
            existing.LastName = profile.LastName;

        existing.MiddleName = profile.MiddleName;

        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
            existing.AvatarUrl = profile.AvatarUrl;

        await _repository.UpdateAsync(existing);

        await _publishEndpoint.Publish(new UserProfileUpdatedMessage
        {
            UserId = existing.UserId,
            FirstName = existing.FirstName,
            LastName = existing.LastName,
            MiddleName = existing.MiddleName
        });

        _logger.LogInformation("Profile main info updated for user {UserId}", userId);

        return Result.Success();
    }

    public async Task<Result> UpdateContactsAsync(Guid userId, ClientProfile profile)
    {
        var existingResult = await GetAsync(userId);
        if (existingResult.IsFailure)
            return Result.Failure(existingResult.Error.Description);

        var existing = existingResult.Value;

        if (!string.IsNullOrWhiteSpace(profile.Phone))
            existing.Phone = profile.Phone;

        if (!string.IsNullOrWhiteSpace(profile.City))
            existing.City = profile.City;

        if (!string.IsNullOrWhiteSpace(profile.CityId))
            existing.CityId = profile.CityId;

        if (!string.IsNullOrWhiteSpace(profile.District))
            existing.District = profile.District;

        await _repository.UpdateAsync(existing);

        _logger.LogInformation("Profile contacts updated for user {UserId}", userId);

        return Result.Success();
    }

    public async Task<Result> CreateAsync(Guid userId, ClientProfile profile)
    {
        var existing = await _repository.GetByUserIdAsync(userId);

        if (existing != null)
        {
            _logger.LogWarning("Profile already exists for user {UserId}", userId);
            return Result.Failure("Profile already exists.");
        }

        profile.Id = Guid.NewGuid();
        profile.UserId = userId;

        await _repository.AddAsync(profile);

        _logger.LogInformation("Profile created for user {UserId}", userId);

        return Result.Success();
    }
}