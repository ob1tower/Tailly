using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Helpers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Common;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Applications;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Application.Service;

public class SpecialistApplicationService : ISpecialistApplicationService
{
    private readonly ISpecialistApplicationRepository _applicationRepository;
    private readonly ISpecialistRepository _specialistRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SpecialistApplicationService> _logger;

    public SpecialistApplicationService(ISpecialistApplicationRepository applicationRepository,
                                        ISpecialistRepository specialistRepository,
                                        IPublishEndpoint publishEndpoint,
                                        ILogger<SpecialistApplicationService> logger)
    {
        _applicationRepository = applicationRepository;
        _specialistRepository = specialistRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<SpecialistApplication, Error>> CreateAsync(SpecialistApplication application)
    {
        if (application == null)
            return Result.Failure<SpecialistApplication, Error>(SpecialistApplicationErrors.InvalidApplication);

        if (!string.IsNullOrWhiteSpace(application.Email))
            application.Email = application.Email.Trim().ToLowerInvariant();

        bool hasActiveApplication = await _applicationRepository.HasActiveApplicationAsync(application.Email);
        if (hasActiveApplication)
        {
            _logger.LogWarning("Duplicate pending application attempt. Email: {Email}", application.Email);
            return Result.Failure<SpecialistApplication, Error>(SpecialistApplicationErrors.ApplicationAlreadyExists);
        }

        bool isAlreadySpecialist = await _specialistRepository.ExistsByEmailAsync(application.Email);
        if (isAlreadySpecialist)
        {
            _logger.LogWarning("User is already a specialist. Email: {Email}", application.Email);
            return Result.Failure<SpecialistApplication, Error>(SpecialistApplicationErrors.AlreadySpecialist);
        }

        application.Id = Guid.NewGuid();
        application.Status = SpecialistApplicationStatus.Pending;
        application.CreatedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _applicationRepository.AddAsync(application);

        _logger.LogInformation("New specialist application created successfully. Id: {Id}, UserId: {UserId}, Email: {Email}",
            application.Id, application.UserId, application.Email);

        return Result.Success<SpecialistApplication, Error>(application);
    }

    public async Task<Result<(List<SpecialistApplication> items, int total), Error>> GetAllAsync(
        int page, int limit, SpecialistApplicationStatus? status = null)
    {
        var (items, total) = await _applicationRepository.GetAllAsync(page, limit, status);

        _logger.LogInformation("Retrieved {Count} specialist applications (page {Page})", items.Count, page);

        return Result.Success<(List<SpecialistApplication> items, int total), Error>((items, total));
    }

    public async Task<Result> AssignInterviewAsync(Guid id, string note, DateTime? interviewDate)
    {
        var app = await _applicationRepository.GetByIdAsync(id);
        if (app == null)
            return Result.Failure(SpecialistApplicationErrors.NotFound.Description);

        if (app.Status == SpecialistApplicationStatus.Rejected)
            return Result.Failure(SpecialistApplicationErrors.CannotChangeRejected.Description);

        if (app.Status != SpecialistApplicationStatus.Pending)
            return Result.Failure(SpecialistApplicationErrors.InvalidStatusTransition.Description);

        app.Status = SpecialistApplicationStatus.InterviewScheduled;
        app.InterviewNote = note;
        app.InterviewDate = interviewDate;
        app.UpdatedAt = DateTime.UtcNow;

        await _applicationRepository.UpdateAsync(app);

        _logger.LogInformation("Interview assigned for application {Id}", id);
        return Result.Success();
    }

    public async Task<Result> RejectAsync(Guid id, string reason)
    {
        var app = await _applicationRepository.GetByIdAsync(id);
        if (app == null)
            return Result.Failure(SpecialistApplicationErrors.NotFound.Description);

        if (app.Status == SpecialistApplicationStatus.Rejected)
            return Result.Failure(SpecialistApplicationErrors.AlreadyProcessed.Description);

        app.Status = SpecialistApplicationStatus.Rejected;
        app.RejectionReason = reason;
        app.UpdatedAt = DateTime.UtcNow;

        await _applicationRepository.UpdateAsync(app);

        _logger.LogInformation("Application {Id} rejected. Reason: {Reason}", id, reason);

        return Result.Success();
    }

    public async Task<Result> ApproveAsync(Guid id)
    {
        var app = await _applicationRepository.GetByIdAsync(id);
        if (app == null)
            return Result.Failure(SpecialistApplicationErrors.NotFound.Description);

        if (app.Status == SpecialistApplicationStatus.Rejected)
            return Result.Failure(SpecialistApplicationErrors.CannotChangeApprove.Description);

        if (app.Status != SpecialistApplicationStatus.InterviewScheduled)
            return Result.Failure(SpecialistApplicationErrors.InvalidStatusTransition.Description);

        app.Status = SpecialistApplicationStatus.Approved;
        app.UpdatedAt = DateTime.UtcNow;

        await _applicationRepository.UpdateAsync(app);

        _logger.LogInformation("Application {Id} approved", id);
        return Result.Success();
    }

    public async Task<Result<Specialist, Error>> AttachSpecialistAccountAsync(Guid applicationId, string reviewedByAdminId)
    {
        var app = await _applicationRepository.GetByIdAsync(applicationId);
        if (app == null)
            return Result.Failure<Specialist, Error>(SpecialistApplicationErrors.NotFound);

        if (app.Status != SpecialistApplicationStatus.Approved)
            return Result.Failure<Specialist, Error>(SpecialistApplicationErrors.InvalidStatusTransition);

        string uniqueSlug = await SpecialistSlugGenerator.GenerateUniqueSlugAsync(
            app.FirstName,
            app.LastName,
            _specialistRepository);

        var specialist = new Specialist
        {
            Id = Guid.NewGuid(),
            UserId = null,                    
            Slug = uniqueSlug,    
            FirstName = app.FirstName,
            LastName = app.LastName,
            MiddleName = app.MiddleName,
            City = app.City,
            Phone = app.Phone,
            Email = app.Email,
            AvatarUrl = app.PhotoUrl,
            About = app.About,
            Description = app.About,
            ExperienceYears = app.ExperienceYears,
            CreatedAt = DateTime.UtcNow,

            Rating = 0,
            ReviewsCount = 0,
            CompletedOrdersCount = 0,
            RepeatOrdersCount = 0
        };

        await _specialistRepository.AddAsync(specialist);

        await _publishEndpoint.Publish(new SpecialistAccountCreated
        {
            ApplicationId = applicationId,
            SpecialistId = specialist.Id,
            Slug = uniqueSlug,           
            Email = app.Email,
            FirstName = app.FirstName,
            LastName = app.LastName,
            MiddleName = app.MiddleName ?? "",
            Phone = app.Phone,
            City = app.City,
            About = app.About ?? "",
            TemporaryPassword = "",
            CreatedByAdminId = reviewedByAdminId
        });

        _logger.LogInformation("Specialist account created from application {ApplicationId}. SpecialistId: {SpecialistId}, Slug: {Slug}",
            applicationId, specialist.Id, uniqueSlug);

        return Result.Success<Specialist, Error>(specialist);
    }
}