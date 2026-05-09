using Tailly.SpecialistService.Application.Dtos.Responses.Home;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Application.Service.Interfaces;

public interface ISpecialistsService
{
    Task<List<Specialist>> SearchAsync(string? cityQuery, string? districtQuery, string? serviceType, decimal? priceMin, decimal? priceMax, int page, int pageSize);
    Task<Specialist?> GetFullProfileByIdAsync(Guid id, ReviewSortType reviewSortType);
    Task<Specialist?> GetFullProfileBySlugAsync(string slug, ReviewSortType reviewSortType);
    Task<List<HomeReviewResponse>> GetHomeReviewsAsync(int? rating, int limit, bool requirePhotos, int minTextLength, int minWords);
}