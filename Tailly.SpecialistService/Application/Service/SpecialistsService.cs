using Microsoft.Extensions.Options;
using Tailly.SpecialistService.Application.Dtos.Responses.Home;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Configurations.Options;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Application.Service;

public class SpecialistsService : ISpecialistsService
{
    private readonly ISpecialistRepository _repository;
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<SpecialistsService> _logger;

    public SpecialistsService(ISpecialistRepository repository,
                              IOptions<ApiSettings> apiSettings,
                              ILogger<SpecialistsService> logger)
    {
        _repository = repository;
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    public async Task<List<Specialist>> SearchAsync(string? cityQuery, string? districtQuery, string? serviceType, string? petType, int? experienceFrom, bool onlyWithReviews, string? sort, decimal? priceMin, decimal? priceMax, int page, int pageSize)
    {
        _logger.LogInformation("Searching specialists. " + "City: {City}, District: {District}, ServiceType: {ServiceType}, " + "PetType: {PetType}, ExperienceFrom: {ExperienceFrom}, " +
                               "OnlyWithReviews: {OnlyWithReviews}, Sort: {Sort}, " + "PriceMin: {PriceMin}, PriceMax: {PriceMax}, Page: {Page}",
                               cityQuery, districtQuery, serviceType, petType, experienceFrom, onlyWithReviews, sort, priceMin, priceMax, page);

        return await _repository.SearchAsync(cityQuery, districtQuery, serviceType, petType, experienceFrom, onlyWithReviews, sort, priceMin, priceMax, page, pageSize);
    }

    public async Task<Specialist?> GetFullProfileByIdAsync(Guid id, ReviewSortType reviewSortType)
    {
        return await _repository.GetFullProfileByIdAsync(id, reviewSortType);
    }

    public async Task<Specialist?> GetFullProfileBySlugAsync(string slug, ReviewSortType reviewSortType)
    {
        return await _repository.GetFullProfileBySlugAsync(slug, reviewSortType);
    }

    public async Task<List<HomeReviewResponse>> GetHomeReviewsAsync(int? rating, int limit, bool requirePhotos, int minTextLength, int minWords)
    {
        var reviews = await _repository.GetHomeReviewsAsync(
            rating,
            limit,
            requirePhotos,
            minTextLength,
            minWords);

        foreach (var review in reviews)
        {
            review.PhotoUrls = review.PhotoUrls
                .Select(x => $"{_apiSettings.BaseUrl}{x}")
                .ToList();
        }

        return reviews;
    }
}