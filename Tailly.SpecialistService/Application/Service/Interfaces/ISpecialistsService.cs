using CSharpFunctionalExtensions;
using Tailly.SpecialistService.Core.Common;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Application.Service.Interfaces;

public interface ISpecialistsService
{
    Task<Result<Specialist, Error>> GetByIdAsync(Guid id);
    Task<Result<Specialist, Error>> UpdateDetailsAsync(string slug, Guid userId, string experienceLabel, int? experienceDurationValue, ExperienceUnit? experienceDurationUnit, HousingType housingType, List<PetSize> petSizes,
                                                                    List<PetAge> petAges, ChildrenPresence hasChildrenUnderTen, List<PetType> petTypes, List<Advantage> advantages, string about, List<ServiceOffer> services);
    Task<Result<Specialist, Error>> UpdateMainAsync(string slug, Guid userId, string firstName, string lastName, string? middleName, string city, string district, string phone, string? avatarUrl);
    Task<Result<Specialist, Error>> GetBySlugAsync(string slug);
    Task<Result<(List<Specialist> specialists, int total), Error>> GetAllAsync(string? cityQuery, string? districtQuery, string? serviceId, decimal? priceMin, decimal? priceMax,
                                                                                             int? experienceMinYears, bool hasReviewsOnly, SpecialistSort sort, int page, int limit);
}