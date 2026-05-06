using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Reviews;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

public interface ISpecialistRepository
{
    Task<Specialist?> GetBySlugAsync(string slug);
    Task<Specialist?> GetByIdAsync(Guid id);
    Task<Specialist?> GetFullProfileBySlugAsync(string slug);
    Task<Specialist?> GetFullProfileByIdAsync(Guid id);
    Task UpdateMainInfoAsync(Guid specialistId, string firstName, string lastName, string? middleName,
                             string city, string district, string phone, string? avatarUrl);
    Task UpdateDetailsAsync(Guid specialistId, Details details);
    Task<ServiceOffer?> GetServiceByIdAsync(Guid serviceId);
    Task AddServiceAsync(Guid specialistId, ServiceOffer service);
    Task UpdateServiceAsync(ServiceOffer service);
    Task DeleteServiceAsync(Guid serviceId);
    Task AddReviewReplyAsync(Guid reviewId, string replyText);
    Task<Review?> GetReviewByIdAsync(Guid reviewId);
    Task<bool> SlugExistsAsync(string slug);
    Task AddAsync(Specialist specialist);
    Task<bool> HasServiceOfTypeAsync(Guid specialistId, ServiceType serviceType);
    Task<List<Specialist>> SearchAsync(string? cityQuery, string? districtQuery, string? serviceType,
                                       decimal? priceMin, decimal? priceMax, int page, int pageSize);
}