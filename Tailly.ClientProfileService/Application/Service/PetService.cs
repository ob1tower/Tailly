using CSharpFunctionalExtensions;
using Tailly.ClientProfileService.Application.Errors;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.ClientProfileService.Core.Common;
using Tailly.ClientProfileService.Core.Models;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Application.Service;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;
    private readonly IClientProfileRepository _profileRepository;
    private readonly ILogger<PetService> _logger;

    public PetService(IPetRepository petRepository,
                      IClientProfileRepository profileRepository,
                      ILogger<PetService> logger)
    {
        _petRepository = petRepository;
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task<Result<List<Pet>, Error>> GetByUserAsync(Guid userId)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            _logger.LogWarning("Profile not found for user {UserId}", userId);
            return Result.Failure<List<Pet>, Error>(ClientProfileErrors.ProfileNotFound);
        }

        var pets = await _petRepository.GetByClientIdAsync(profile.Id);

        return Result.Success<List<Pet>, Error>(pets);
    }

    public async Task<Result<Pet, Error>> GetByIdAsync(Guid userId, Guid petId)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            _logger.LogWarning("Profile not found for user {UserId}", userId);
            return Result.Failure<Pet, Error>(ClientProfileErrors.ProfileNotFound);
        }

        var pet = await _petRepository.GetByIdAsync(petId);
        if (pet == null)
        {
            _logger.LogWarning("Pet not found {PetId}", petId);
            return Result.Failure<Pet, Error>(ClientProfileErrors.PetNotFound);
        }

        if (pet.ClientId != profile.Id)
        {
            _logger.LogWarning("Access denied to pet {PetId} for user {UserId}", petId, userId);
            return Result.Failure<Pet, Error>(ClientProfileErrors.AccessDenied);
        }

        return Result.Success<Pet, Error>(pet);
    }

    public async Task<Result<Pet, Error>> CreateAsync(Guid userId, Pet pet)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            _logger.LogWarning("Profile not found for user {UserId}", userId);
            return Result.Failure<Pet, Error>(ClientProfileErrors.ProfileNotFound);
        }

        pet.Id = Guid.NewGuid();
        pet.ClientId = profile.Id;

        await _petRepository.AddAsync(pet);

        _logger.LogInformation("Pet {PetId} created for user {UserId}", pet.Id, userId);

        return Result.Success<Pet, Error>(pet);
    }

    public async Task<Result> UpdateAsync(Guid userId, Pet pet)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            _logger.LogWarning("Profile not found for user {UserId}", userId);
            return Result.Failure(ClientProfileErrors.ProfileNotFound.Description);
        }

        var existing = await _petRepository.GetByIdAsync(pet.Id);
        if (existing == null)
        {
            _logger.LogWarning("Pet not found {PetId}", pet.Id);
            return Result.Failure(ClientProfileErrors.PetNotFound.Description);
        }

        if (existing.ClientId != profile.Id)
        {
            _logger.LogWarning("Access denied to update pet {PetId} for user {UserId}", pet.Id, userId);
            return Result.Failure(ClientProfileErrors.AccessDenied.Description);
        }

        existing.Name = pet.Name;
        existing.PhotoUrl = pet.PhotoUrl;
        existing.Type = pet.Type;
        existing.BreedId = pet.BreedId;
        existing.AgeYears = pet.AgeYears;
        existing.AgeMonths = pet.AgeMonths;
        existing.Size = pet.Size;
        existing.Gender = pet.Gender;
        existing.ToOtherPets = pet.ToOtherPets;
        existing.ToKidsUnder10 = pet.ToKidsUnder10;
        existing.StaysHomeAlone = pet.StaysHomeAlone;
        existing.Vaccinated = pet.Vaccinated;
        existing.Notes = pet.Notes;

        await _petRepository.UpdateAsync(existing);

        _logger.LogInformation("Pet {PetId} updated for user {UserId}", pet.Id, userId);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid petId)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            _logger.LogWarning("Profile not found for user {UserId}", userId);
            return Result.Failure(ClientProfileErrors.ProfileNotFound.Description);
        }

        var pet = await _petRepository.GetByIdAsync(petId);
        if (pet == null)
        {
            _logger.LogWarning("Pet not found {PetId}", petId);
            return Result.Failure(ClientProfileErrors.PetNotFound.Description);
        }

        if (pet.ClientId != profile.Id)
        {
            _logger.LogWarning("Access denied to delete pet {PetId} for user {UserId}", petId, userId);
            return Result.Failure(ClientProfileErrors.AccessDenied.Description);
        }

        await _petRepository.DeleteAsync(petId);

        _logger.LogInformation("Pet {PetId} deleted for user {UserId}", petId, userId);

        return Result.Success();
    }
}