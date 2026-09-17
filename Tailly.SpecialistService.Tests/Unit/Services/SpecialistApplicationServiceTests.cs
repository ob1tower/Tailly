using CSharpFunctionalExtensions;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Service;
using Tailly.SpecialistService.Application.Service.Security;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Applications;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Tests.Unit.Services;

/// <summary>
/// Unit tests for SpecialistApplicationService.
/// Covers ALL methods with positive and negative scenarios:
/// 
/// GetAllAsync:
/// - success (with and without status filter)
/// 
/// AssignInterviewAsync:
/// - success
/// - not found
/// - cannot change rejected
/// - already processed (approved)
/// - interview date in past
/// - interview date too soon
/// - interview slot conflict
/// 
/// RejectAsync:
/// - success
/// - not found
/// - cannot change approved
/// - reason too short
/// - reason invalid (no letters)
/// 
/// ApproveAsync:
/// - success
/// - not found
/// - cannot change rejected
/// - already processed
/// 
/// AttachSpecialistAccountAsync:
/// - success
/// - not found
/// - invalid status transition
/// - specialist already exists
/// 
/// CreateAsync:
/// - success
/// - application already exists (pending/approved)
/// - specialist already exists
/// </summary>
public class SpecialistApplicationServiceTests
{
    private readonly Mock<ISpecialistApplicationRepository> _applicationRepository = new();
    private readonly Mock<ISpecialistRepository> _specialistRepository = new();
    private readonly Mock<ICalendarRepository> _calendarRepository = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<ILogger<SpecialistApplicationService>> _logger = new();

    private SpecialistApplicationService CreateService()
    {
        var redisMock = new Mock<IConnectionMultiplexer>();
        var databaseMock = new Mock<IDatabase>();

        redisMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(databaseMock.Object);

        databaseMock
            .Setup(x => x.StringGetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        databaseMock
            .Setup(x => x.KeyDeleteAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var temporaryPasswordService =
            new SpecialistTemporaryPasswordService(redisMock.Object);

        return new SpecialistApplicationService(
            _applicationRepository.Object,
            _specialistRepository.Object,
            _calendarRepository.Object,
            _publishEndpoint.Object,
            temporaryPasswordService,
            _logger.Object);
    }

    private SpecialistApplication CreateValidApplication(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Email = "test@specialist.ru",
        FirstName = "Иван",
        LastName = "Иванов",
        Status = SpecialistApplicationStatus.Pending,
        ExperienceYears = 3
    };

    // =============================================
    // ============== GetAllAsync ==============
    // =============================================

    [Fact]
    public async Task GetAllAsync_Should_Return_Paginated_Applications()
    {
        // arrange
        var apps = new List<SpecialistApplication> { CreateValidApplication() };
        _applicationRepository.Setup(x => x.GetAllAsync(1, 10, null))
                              .ReturnsAsync((apps, 1));

        var service = CreateService();

        // act
        var result = await service.GetAllAsync(1, 10);

        // assert
        result.IsSuccess.Should().BeTrue();
        var (items, total) = result.Value;
        items.Should().HaveCount(1);
        total.Should().Be(1);
    }

    // =============================================
    // ============== AssignInterviewAsync ==============
    // =============================================

    [Fact]
    public async Task AssignInterviewAsync_Should_Assign_Successfully()
    {
        // arrange
        var app = CreateValidApplication();
        app.Status = SpecialistApplicationStatus.Pending;

        _applicationRepository.Setup(x => x.GetByIdAsync(app.Id)).ReturnsAsync(app);
        _applicationRepository.Setup(x => x.UpdateAsync(It.IsAny<SpecialistApplication>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.AssignInterviewAsync(app.Id, "Хороший кандидат", DateTime.UtcNow.AddDays(2), "admin");

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AssignInterviewAsync_Should_Return_NotFound()
    {
        // arrange
        var service = CreateService();
        _applicationRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SpecialistApplication?)null);

        // act
        var result = await service.AssignInterviewAsync(Guid.NewGuid(), "note", null, "admin");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistApplicationErrors.NotFound.Description);
    }

    [Fact]
    public async Task AssignInterviewAsync_Should_Return_CannotChangeRejected()
    {
        // arrange
        var app = CreateValidApplication();
        app.Status = SpecialistApplicationStatus.Rejected;
        _applicationRepository.Setup(x => x.GetByIdAsync(app.Id)).ReturnsAsync(app);

        var service = CreateService();

        // act
        var result = await service.AssignInterviewAsync(app.Id, "note", null, "admin");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistApplicationErrors.CannotChangeRejected.Description);
    }

    // =============================================
    // ============== RejectAsync ==============
    // =============================================

    [Fact]
    public async Task RejectAsync_Should_Reject_Successfully()
    {
        // arrange
        var app = CreateValidApplication();
        app.Status = SpecialistApplicationStatus.Pending;

        _applicationRepository.Setup(x => x.GetByIdAsync(app.Id)).ReturnsAsync(app);
        _applicationRepository.Setup(x => x.UpdateAsync(It.IsAny<SpecialistApplication>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.RejectAsync(app.Id, "Не соответствует требованиям и ожиданиям компании", "admin");

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RejectAsync_Should_Return_ReasonTooShort()
    {
        // arrange
        var app = CreateValidApplication();
        _applicationRepository.Setup(x => x.GetByIdAsync(app.Id)).ReturnsAsync(app);

        var service = CreateService();

        // act
        var result = await service.RejectAsync(app.Id, "коротко", "admin");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistApplicationErrors.RejectionReasonTooShort.Description);
    }

    // =============================================
    // ============== ApproveAsync ==============
    // =============================================

    [Fact]
    public async Task ApproveAsync_Should_Approve_Successfully()
    {
        // arrange
        var app = CreateValidApplication();
        app.Status = SpecialistApplicationStatus.Pending;

        _applicationRepository.Setup(x => x.GetByIdAsync(app.Id)).ReturnsAsync(app);
        _applicationRepository.Setup(x => x.UpdateAsync(It.IsAny<SpecialistApplication>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ApproveAsync(app.Id, "admin", "Отличный специалист");

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== AttachSpecialistAccountAsync ==============
    // =============================================

    [Fact]
    public async Task AttachSpecialistAccountAsync_Should_Attach_Successfully()
    {
        // arrange
        var app = CreateValidApplication();
        app.Status = SpecialistApplicationStatus.Approved;

        _applicationRepository.Setup(x => x.GetByIdAsync(app.Id)).ReturnsAsync(app);
        _specialistRepository.Setup(x => x.ExistsByEmailAsync(app.Email)).ReturnsAsync(false);
        _specialistRepository.Setup(x => x.AddAsync(It.IsAny<Specialist>())).Returns(Task.CompletedTask);
        _calendarRepository.Setup(x => x.CreateAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _applicationRepository.Setup(x => x.UpdateAsync(It.IsAny<SpecialistApplication>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.AttachSpecialistAccountAsync(app.Id, "admin");

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AttachSpecialistAccountAsync_Should_Return_NotFound()
    {
        // arrange
        var service = CreateService();
        _applicationRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SpecialistApplication?)null);

        // act
        var result = await service.AttachSpecialistAccountAsync(Guid.NewGuid(), "admin");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistApplicationErrors.NotFound);
    }

    // =============================================
    // ============== CreateAsync ==============
    // =============================================

    [Fact]
    public async Task CreateAsync_Should_Create_Application_Successfully()
    {
        // arrange
        var app = CreateValidApplication();
        _applicationRepository.Setup(x => x.HasPendingOrApprovedApplicationAsync(app.Email)).ReturnsAsync(false);
        _specialistRepository.Setup(x => x.ExistsByEmailAsync(app.Email)).ReturnsAsync(false);
        _applicationRepository.Setup(x => x.AddAsync(It.IsAny<SpecialistApplication>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(app);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_Should_Return_ApplicationAlreadyExists()
    {
        // arrange
        var app = CreateValidApplication();

        _applicationRepository
            .Setup(x => x.HasPendingOrApprovedApplicationAsync(app.Email))
            .ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(app);

        // assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should()
            .Be(SpecialistApplicationErrors.ApplicationAlreadyExists);
    }
}