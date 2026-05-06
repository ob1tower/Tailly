using CSharpFunctionalExtensions;
using Tailly.SpecialistService.Core.Common;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Application.Service.Interfaces;

public interface ISpecialistProfileService
{
    Task<Result> AddServiceAsync(string slug, Guid specialistId, ServiceOffer service);
    Task<Result> DeleteServiceAsync(string slug, Guid specialistId, Guid serviceId);
    Task<Result> UpdateDetailsAsync(string slug, Guid specialistId, Details details);
    Task<Result> UpdateMainInfoAsync(string slug, Guid specialistId, Guid userId, string firstName, string lastName, string? middleName, string city, string district, string phone, string? avatarUrl);
    Task<Result> UpdateServiceAsync(string slug, Guid specialistId, ServiceOffer service);
    Task<Result<bool, Error>> ReplyToReviewAsync(string slug, Guid specialistId, Guid reviewId, string text);
}