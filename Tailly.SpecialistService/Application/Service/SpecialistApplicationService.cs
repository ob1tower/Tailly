using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Helpers;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Application.Service.Security;
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
    private readonly SpecialistTemporaryPasswordService _temporaryPasswordService;
    private readonly ILogger<SpecialistApplicationService> _logger;

    public SpecialistApplicationService(ISpecialistApplicationRepository applicationRepository,
                                        ISpecialistRepository specialistRepository,
                                        IPublishEndpoint publishEndpoint,
                                        SpecialistTemporaryPasswordService temporaryPasswordService,
                                        ILogger<SpecialistApplicationService> logger)
    {
        _applicationRepository = applicationRepository;
        _specialistRepository = specialistRepository;
        _publishEndpoint = publishEndpoint;
        _temporaryPasswordService = temporaryPasswordService;
        _logger = logger;
    }

    public async Task<Result<(List<SpecialistApplication> items, int total), Error>> GetAllAsync(
            int page, int limit, SpecialistApplicationStatus? status = null)
    {
        var (items, total) = await _applicationRepository.GetAllAsync(page, limit, status);
        return Result.Success<(List<SpecialistApplication>, int), Error>((items, total));
    }

    public async Task<Result> AssignInterviewAsync(Guid id, string note, DateTime? interviewDate, string reviewedBy)
    {
        var app = await _applicationRepository.GetByIdAsync(id);
        if (app == null)
            return Result.Failure(SpecialistApplicationErrors.NotFound.Description);

        if (app.Status == SpecialistApplicationStatus.Rejected)
            return Result.Failure(SpecialistApplicationErrors.CannotChangeRejected.Description);

        if (app.Status == SpecialistApplicationStatus.Approved)
            return Result.Failure(SpecialistApplicationErrors.AlreadyProcessed.Description);

        if (interviewDate.HasValue)
        {
            var utcDate = DateTimeHelper.NormalizeToUtc(interviewDate.Value);

            if (utcDate < DateTime.UtcNow)
                return Result.Failure(SpecialistApplicationErrors.InterviewDateInPast.Description);

            if (utcDate < DateTime.UtcNow.AddHours(1))
                return Result.Failure(SpecialistApplicationErrors.InterviewDateTooSoon.Description);

            if (await _applicationRepository.HasInterviewConflictAsync(reviewedBy, utcDate))
                return Result.Failure(SpecialistApplicationErrors.InterviewSlotConflict.Description);
        }

        app.Status = SpecialistApplicationStatus.InterviewScheduled;
        app.InterviewNote = note;
        app.InterviewDate = interviewDate.HasValue
            ? DateTimeHelper.NormalizeToUtc(interviewDate.Value)
            : null;
        app.UpdatedAt = DateTime.UtcNow;
        app.ReviewComment = note;
        app.ReviewedBy = reviewedBy;

        await _applicationRepository.UpdateAsync(app);
        _logger.LogInformation("Interview assigned for application {Id}", id);
        return Result.Success();
    }

    public async Task<Result> RejectAsync(Guid id, string reason, string reviewedBy)
    {
        var app = await _applicationRepository.GetByIdAsync(id);
        if (app == null)
            return Result.Failure(SpecialistApplicationErrors.NotFound.Description);

        if (app.Status == SpecialistApplicationStatus.Approved)
            return Result.Failure(SpecialistApplicationErrors.CannotChangeApprove.Description);

        if (string.IsNullOrWhiteSpace(reason) || reason.Length < 15)
            return Result.Failure(SpecialistApplicationErrors.RejectionReasonTooShort.Description);

        if (!reason.Any(char.IsLetter))
            return Result.Failure(SpecialistApplicationErrors.RejectionReasonInvalid.Description);

        app.Status = SpecialistApplicationStatus.Rejected;
        app.RejectionReason = reason;
        app.UpdatedAt = DateTime.UtcNow;
        app.ReviewComment = reason;
        app.ReviewedBy = reviewedBy;

        await _applicationRepository.UpdateAsync(app);
        _logger.LogInformation("Application {Id} rejected", id);
        return Result.Success();
    }

    public async Task<Result> ApproveAsync(Guid id, string reviewedBy, string? reviewComment = null)
    {
        var app = await _applicationRepository.GetByIdAsync(id);
        if (app == null)
            return Result.Failure(SpecialistApplicationErrors.NotFound.Description);

        if (app.Status == SpecialistApplicationStatus.Rejected)
            return Result.Failure(SpecialistApplicationErrors.CannotChangeRejected.Description);

        if (app.Status == SpecialistApplicationStatus.Approved)
            return Result.Failure(SpecialistApplicationErrors.AlreadyProcessed.Description);

        app.Status = SpecialistApplicationStatus.Approved;
        app.ReviewedBy = reviewedBy;
        app.ReviewComment = reviewComment;   
        app.UpdatedAt = DateTime.UtcNow;

        await _applicationRepository.UpdateAsync(app);
        _logger.LogInformation("Application {Id} approved.", id);
        return Result.Success();
    }

    public async Task<Result<(Specialist Specialist, string? TemporaryPassword), Error>>AttachSpecialistAccountAsync(Guid applicationId, string reviewedByAdminId)
    {
        var app = await _applicationRepository.GetByIdAsync(applicationId);
        if (app == null)
            return Result.Failure<(Specialist Specialist, string? TemporaryPassword), Error>(SpecialistApplicationErrors.NotFound);

        if (app.Status != SpecialistApplicationStatus.Approved)
            return Result.Failure<(Specialist Specialist, string? TemporaryPassword), Error>(SpecialistApplicationErrors.InvalidStatusTransition);

        if (app.CreatedSpecialistId.HasValue)
            return Result.Failure<(Specialist Specialist, string? TemporaryPassword), Error>(SpecialistApplicationErrors.SpecialistAlreadyExists);

        string? temporaryPassword = null;

        var slug = await SpecialistSlugGenerator.GenerateUniqueSlugAsync(
            app.FirstName, app.LastName, _specialistRepository);

        var specialistId = Guid.NewGuid();

        var specialist = new Specialist
        {
            Id = specialistId,
            UserId = Guid.Empty,
            Slug = slug,
            FirstName = app.FirstName,
            LastName = app.LastName,
            MiddleName = app.MiddleName,
            District = app.DistrictPreferences,
            City = app.City,
            ExperienceYears = app.ExperienceYears,
            Email = app.Email,
            Phone = app.Phone,
            Rating = 0,
            ReviewsCount = 0,
            CompletedOrdersCount = 0,
            RepeatOrdersCount = 0,
            CreatedAt = DateTime.UtcNow,

            Details = new Details
            {
                About = app.About ?? "",
                HousingType = HousingType.Apartment,
                HasChildrenUnderTen = ChildrenPolicy.No,

                PetSizes = [PetSize.Kg5To10],
                PetAges = [PetAge.Adult],
                PetTypes = [PetType.Dog]
            }
        };

        await _specialistRepository.AddAsync(specialist);

        app.CreatedSpecialistId = specialist.Id;
        app.CreatedSpecialistSlug = specialist.Slug;
        app.SpecialistAccountCreatedAt = DateTime.UtcNow;
        app.UpdatedAt = DateTime.UtcNow;
        app.ReviewedBy = reviewedByAdminId;

        await _applicationRepository.UpdateAsync(app);

        await _publishEndpoint.Publish(new SpecialistAccountCreated
        {
            ApplicationId = applicationId,
            SpecialistId = specialistId,
            Slug = slug,
            Email = app.Email,
            FirstName = app.FirstName,
            LastName = app.LastName,
            MiddleName = app.MiddleName ?? "",
            Phone = app.Phone,
            City = app.City,
            About = app.About ?? "",
            TemporaryPassword = temporaryPassword,
            CreatedByAdminId = reviewedByAdminId
        });
        
        await Task.Delay(1500);

        temporaryPassword = await _temporaryPasswordService.GetAsync(specialistId);

        if (!string.IsNullOrWhiteSpace(temporaryPassword))
        {
            await _temporaryPasswordService.RemoveAsync(specialistId);
        }

        _logger.LogInformation("SpecialistAccountCreated published for application {ApplicationId}", applicationId);

        return Result.Success<(Specialist, string?), Error>((specialist, temporaryPassword));
    }

    public async Task<Result<SpecialistApplication, Error>> CreateAsync(SpecialistApplication application)
    {
        var hasActiveOrApproved = await _applicationRepository.HasPendingOrApprovedApplicationAsync(application.Email);
        if (hasActiveOrApproved)
        {
            _logger.LogWarning("User already has pending or approved application. Email: {Email}", application.Email);
            return Result.Failure<SpecialistApplication, Error>(SpecialistApplicationErrors.ApplicationAlreadyExists);
        }

        var specialistExists = await _specialistRepository.ExistsByEmailAsync(application.Email);
        if (specialistExists)
        {
            _logger.LogWarning("Specialist with this email already exists. Email: {Email}", application.Email);
            return Result.Failure<SpecialistApplication, Error>(SpecialistApplicationErrors.SpecialistAlreadyExists);
        }

        application.CreatedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;
        application.Status = SpecialistApplicationStatus.Pending;

        await _applicationRepository.AddAsync(application);
        _logger.LogInformation("New specialist application created: {Email}", application.Email);
        return Result.Success<SpecialistApplication, Error>(application);
    }
}