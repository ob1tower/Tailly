using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Common;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Application.Service;

public class SpecialistProfileService : ISpecialistProfileService
{
    private readonly ISpecialistRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SpecialistProfileService> _logger;

    public SpecialistProfileService(ISpecialistRepository repository,
                                    IPublishEndpoint publishEndpoint,
                                    ILogger<SpecialistProfileService> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result> UpdateMainInfoAsync(string slug, Guid specialistId, Guid userId, string firstName, string lastName,
           string? middleName, string city, string district, string phone, string? avatarUrl)
    {
        var specialistBySlug = await _repository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        var specialist = await _repository.GetByIdAsync(specialistId);
        if (specialist == null)
        {
            _logger.LogWarning("UpdateMainInfo failed. Specialist not found. SpecialistId: {SpecialistId}", specialistId);
            return Result.Failure(SpecialistErrors.SpecialistNotFound.Description);
        }

        if (specialist.UserId != userId)
        {
            _logger.LogWarning("UpdateMainInfo forbidden. User {UserId} tried to edit specialist {SpecialistId}", userId, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        await _repository.UpdateMainInfoAsync(specialistId, firstName, lastName, middleName, city, district, phone, avatarUrl);

        await _publishEndpoint.Publish(new UserProfileUpdatedMessage
        {
            UserId = specialist.UserId,
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            SpecialistSlug = specialist.Slug
        });

        _logger.LogInformation("Main info updated successfully for specialist {SpecialistId}", specialistId);
        return Result.Success();
    }

    public async Task<Result> UpdateDetailsAsync(string slug, Guid specialistId, Details details)
    {
        var specialistBySlug = await _repository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        var specialist = await _repository.GetByIdAsync(specialistId);
        if (specialist == null)
        {
            _logger.LogWarning("UpdateDetails failed. Specialist not found. SpecialistId: {SpecialistId}", specialistId);
            return Result.Failure(SpecialistErrors.SpecialistNotFound.Description);
        }

        await _repository.UpdateDetailsAsync(specialistId, details);
        _logger.LogInformation("Details updated for specialist {SpecialistId}", specialistId);
        return Result.Success();
    }

    public async Task<Result> AddServiceAsync(string slug, Guid specialistId, ServiceOffer service)
    {
        var specialistBySlug = await _repository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        var specialist = await _repository.GetByIdAsync(specialistId);
        if (specialist == null)
        {
            _logger.LogWarning("AddService failed. Specialist not found. SpecialistId: {SpecialistId}", specialistId);
            return Result.Failure(SpecialistErrors.SpecialistNotFound.Description);
        }

        bool alreadyHasThisType = await _repository.HasServiceOfTypeAsync(specialistId, service.Name);
        if (alreadyHasThisType)
        {
            _logger.LogWarning("Specialist already has a service of type {ServiceType}. SpecialistId: {SpecialistId}",
                service.Name, specialistId);
            return Result.Failure(SpecialistErrors.ServiceTypeAlreadyExists.Description);
        }

        await _repository.AddServiceAsync(specialistId, service);
        _logger.LogInformation("Service added for specialist {SpecialistId}", specialistId);
        return Result.Success();
    }

    public async Task<Result> UpdateServiceAsync(string slug, Guid specialistId, ServiceOffer service)
    {
        var specialistBySlug = await _repository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        var existingService = await _repository.GetServiceByIdAsync(service.Id);
        if (existingService == null)
        {
            _logger.LogWarning("UpdateService failed. Service not found. ServiceId: {ServiceId}", service.Id);
            return Result.Failure(SpecialistErrors.ServiceNotFound.Description);
        }

        if (existingService.SpecialistId != specialistId)
        {
            _logger.LogWarning("UpdateService failed. Service does not belong to specialist. ServiceId: {ServiceId}, SpecialistId: {SpecialistId}", service.Id, specialistId);
            return Result.Failure(SpecialistErrors.ServiceDoesNotBelongToSpecialist.Description);
        }

        if (existingService.Name != service.Name)
        {
            bool alreadyHasThisType = await _repository.HasServiceOfTypeAsync(specialistId, service.Name);
            if (alreadyHasThisType)
            {
                _logger.LogWarning("Cannot change service type to {NewType} because it already exists. ServiceId: {ServiceId}",
                    service.Name, service.Id);
                return Result.Failure(SpecialistErrors.ServiceTypeAlreadyExists.Description);
            }
        }

        await _repository.UpdateServiceAsync(service);
        _logger.LogInformation("Service updated. ServiceId: {ServiceId}", service.Id);
        return Result.Success();
    }

    public async Task<Result> DeleteServiceAsync(string slug, Guid specialistId, Guid serviceId)
    {
        var specialistBySlug = await _repository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        var existingService = await _repository.GetServiceByIdAsync(serviceId);
        if (existingService == null)
        {
            _logger.LogWarning("DeleteService failed. Service not found. ServiceId: {ServiceId}", serviceId);
            return Result.Failure(SpecialistErrors.ServiceNotFound.Description);
        }

        if (existingService.SpecialistId != specialistId)
        {
            _logger.LogWarning("DeleteService failed. Service does not belong to specialist. ServiceId: {ServiceId}, SpecialistId: {SpecialistId}", serviceId, specialistId);
            return Result.Failure(SpecialistErrors.ServiceDoesNotBelongToSpecialist.Description);
        }

        await _repository.DeleteServiceAsync(serviceId);
        _logger.LogInformation("Service deleted. ServiceId: {ServiceId}", serviceId);
        return Result.Success();
    }

    public async Task<Result<bool, Error>> ReplyToReviewAsync(string slug, Guid specialistId, Guid reviewId, string text)
    {
        var specialistBySlug = await _repository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure<bool, Error>(SpecialistErrors.Forbidden);
        }

        var review = await _repository.GetReviewByIdAsync(reviewId);

        if (review == null)
        {
            _logger.LogWarning("Review not found. ReviewId: {ReviewId}", reviewId);
            return Result.Failure<bool, Error>(SpecialistErrors.ReviewNotFound);
        }

        if (review.SpecialistId != specialistId)
        {
            _logger.LogWarning("Specialist tried to reply to a review that doesn't belong to him");
            return Result.Failure<bool, Error>(SpecialistErrors.ReviewDoesNotBelongToSpecialist);
        }

        await _repository.AddReviewReplyAsync(reviewId, text.Trim());

        _logger.LogInformation("Reply added to review {ReviewId} by specialist {SpecialistId}", reviewId, specialistId);

        return Result.Success<bool, Error>(true);
    }
}