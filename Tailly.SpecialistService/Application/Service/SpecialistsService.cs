using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Common;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Application.Service;

public class SpecialistsService : ISpecialistsService
{
    private readonly ISpecialistRepository _repository;
    private readonly ILogger<SpecialistsService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public SpecialistsService(ISpecialistRepository repository,
                             ILogger<SpecialistsService> logger,
                             IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<Specialist, Error>> GetBySlugAsync(string slug)
    {
        slug = slug.Trim().ToLowerInvariant();

        var specialist = await _repository.GetBySlugAsync(slug);

        if (specialist == null)
        {
            _logger.LogWarning("Specialist not found: {Slug}", slug);
            return Result.Failure<Specialist, Error>(SpecialistErrors.NotFound);
        }

        return Result.Success<Specialist, Error>(specialist);
    }

    public async Task<Result<Specialist, Error>> GetByIdAsync(Guid id)
    {
        var specialist = await _repository.GetByIdAsync(id);

        if (specialist == null)
        {
            _logger.LogWarning("Specialist not found: {Id}", id);
            return Result.Failure<Specialist, Error>(SpecialistErrors.NotFound);
        }

        return Result.Success<Specialist, Error>(specialist);
    }

    public async Task<Result<(List<Specialist> specialists, int total), Error>> GetAllAsync(string? cityQuery, string? districtQuery, string? serviceId, decimal? priceMin, decimal? priceMax,
                                                                                             int? experienceMinYears, bool hasReviewsOnly, SpecialistSort sort, int page, int limit)
    {
        cityQuery = string.IsNullOrWhiteSpace(cityQuery)
            ? null
            : cityQuery.Trim().ToLowerInvariant();

        districtQuery = string.IsNullOrWhiteSpace(districtQuery)
            ? null
            : districtQuery.Trim().ToLowerInvariant();

        if (priceMin.HasValue && priceMax.HasValue && priceMin > priceMax)
        {
            return Result.Failure<(List<Specialist>, int), Error>(SpecialistErrors.InvalidPriceRange);
        }

        Guid? serviceGuid = null;

        if (!string.IsNullOrWhiteSpace(serviceId))
        {
            if (!Guid.TryParse(serviceId, out var parsed))
            {
                return Result.Failure<(List<Specialist>, int), Error>(SpecialistErrors.InvalidServiceId);
            }

            serviceGuid = parsed;
        }

        var (specialists, total) = await _repository.GetAllAsync(
            cityQuery,
            districtQuery,
            serviceGuid,
            priceMin,
            priceMax,
            experienceMinYears,
            hasReviewsOnly,
            sort,
            page,
            limit);

        return Result.Success<(List<Specialist>, int), Error>((specialists, total));
    }

    public async Task<Result<Specialist, Error>> UpdateMainAsync(string slug, Guid userId, string firstName, string lastName, string? middleName, string city, string district, string phone, string? avatarUrl)
    {
        slug = slug.Trim().ToLowerInvariant();

        _logger.LogInformation("UpdateMainAsync started for slug: {Slug}", slug);

        var specialist = await _repository.GetBySlugAsync(slug);

        if (specialist == null)
        {
            _logger.LogWarning("Specialist not found: {Slug}", slug);
            return Result.Failure<Specialist, Error>(SpecialistErrors.NotFound);
        }

        if (specialist.UserId != userId)
        {
            _logger.LogWarning("Forbidden update. UserId={UserId}", userId);
            return Result.Failure<Specialist, Error>(SpecialistErrors.Forbidden);
        }

        specialist.FirstName = firstName.Trim();
        specialist.LastName = lastName.Trim();
        specialist.MiddleName = middleName?.Trim();
        specialist.City = city.Trim();
        specialist.District = district.Trim();
        specialist.Phone = phone.Trim();

        if (avatarUrl == "")
        {
            specialist.AvatarUrl = null;
        }
        else if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            if (!avatarUrl.StartsWith("/uploads/"))
            {
                _logger.LogWarning("Invalid avatar url: {Url}", avatarUrl);
                return Result.Failure<Specialist, Error>(SpecialistErrors.InvalidAvatar);
            }

            specialist.AvatarUrl = avatarUrl;
        }

        await _repository.UpdateAsync(specialist);

        await _publishEndpoint.Publish(new UserProfileUpdatedMessage
        {
            UserId = specialist.UserId.Value,
            FirstName = specialist.FirstName,
            LastName = specialist.LastName,
            MiddleName = specialist.MiddleName,
            SpecialistSlug = specialist.Slug
        });

        _logger.LogInformation("UpdateMainAsync success for slug: {Slug}", slug);

        return Result.Success<Specialist, Error>(specialist);
    }

    public async Task<Result<Specialist, Error>> UpdateDetailsAsync(string slug, Guid userId, string experienceLabel, int? experienceDurationValue, ExperienceUnit? experienceDurationUnit, HousingType housingType, List<PetSize> petSizes,
                                                                    List<PetAge> petAges, ChildrenPresence hasChildrenUnderTen, List<PetType> petTypes, List<Advantage> advantages, string about, List<ServiceOffer> services)
    {
        var specialist = await _repository.GetBySlugAsync(slug);
        if (specialist == null)
            return Result.Failure<Specialist, Error>(SpecialistErrors.NotFound);

        if (specialist.UserId != userId)
            return Result.Failure<Specialist, Error>(SpecialistErrors.Forbidden);

        specialist.Details ??= new Details();

        specialist.Details.ExperienceLabel = experienceLabel;
        specialist.Details.ExperienceDurationValue = experienceDurationValue;
        specialist.Details.ExperienceDurationUnit = experienceDurationUnit;     
        specialist.Details.HousingType = housingType;                           
        specialist.Details.HasChildrenUnderTen = hasChildrenUnderTen;            
        specialist.Details.About = about;

        specialist.PetTypes = petTypes;     
        specialist.PetSizes = petSizes;
        specialist.PetAges = petAges;
        specialist.Advantages = advantages;

        specialist.Services.Clear();

        foreach (var service in services)
        {
            service.SpecialistId = specialist.Id;
            specialist.Services.Add(service);
        }

        await _repository.UpdateAsync(specialist);

        _logger.LogInformation("Specialist details updated. Slug: {Slug}, UserId: {UserId}", slug, userId);

        return Result.Success<Specialist, Error>(specialist);
    }
}
