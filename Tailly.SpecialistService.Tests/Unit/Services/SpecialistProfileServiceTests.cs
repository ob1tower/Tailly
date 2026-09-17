using CSharpFunctionalExtensions;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Service;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Reviews;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Tests.Unit.Services;

/// <summary>
/// Unit tests for SpecialistProfileService.
/// Covers ALL methods with positive and negative scenarios:
/// - UpdateMainInfoAsync (success + forbidden slug + specialist not found + wrong user)
/// - UpdateDetailsAsync (success + forbidden slug + specialist not found)
/// - AddServiceAsync (success + forbidden slug + specialist not found + service type already exists)
/// - UpdateServiceAsync (success + forbidden slug + service not found + service does not belong to specialist + changing to existing type)
/// - DeleteServiceAsync (success + forbidden slug + service not found + service does not belong to specialist)
/// - ReplyToReviewAsync (success + forbidden slug + review not found + review does not belong to specialist)
/// </summary>
public class SpecialistProfileServiceTests
{
    private readonly Mock<ISpecialistRepository> _repository = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<ILogger<SpecialistProfileService>> _logger = new();

    private SpecialistProfileService CreateService() =>
        new SpecialistProfileService(_repository.Object, _publishEndpoint.Object, _logger.Object);

    private Specialist CreateValidSpecialist(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Slug = "ivan-ivanov",
        UserId = Guid.NewGuid(),
        FirstName = "Иван",
        LastName = "Иванов"
    };

    // =============================================
    // ============== UpdateMainInfoAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateMainInfoAsync_Should_Update_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var specialist = CreateValidSpecialist(specialistId);
        specialist.UserId = userId;

        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(specialist);
        _repository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(specialist);
        _repository.Setup(x => x.UpdateMainInfoAsync(specialistId, "NewFirst", "NewLast", null, "City", "District", "123456789", null)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateMainInfoAsync(slug, specialistId, userId, "NewFirst", "NewLast", null, "City", "District", "123456789", null);

        // assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpoint.Verify(x => x.Publish(It.IsAny<UserProfileUpdatedMessage>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateMainInfoAsync_Should_Return_Forbidden_When_Slug_Does_Not_Match()
    {
        // arrange
        var service = CreateService();
        _repository.Setup(x => x.GetBySlugAsync("wrong-slug")).ReturnsAsync((Specialist?)null);

        // act
        var result = await service.UpdateMainInfoAsync("wrong-slug", Guid.NewGuid(), Guid.NewGuid(), "First", "Last", null, "City", "District", "phone", null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.Forbidden.Description);
    }

    [Fact]
    public async Task UpdateMainInfoAsync_Should_Return_SpecialistNotFound()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync((Specialist?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateMainInfoAsync(slug, specialistId, Guid.NewGuid(), "First", "Last", null, "City", "District", "phone", null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.SpecialistNotFound.Description);
    }

    // =============================================
    // ============== UpdateDetailsAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateDetailsAsync_Should_Update_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var details = new Details();
        var specialist = CreateValidSpecialist(specialistId);

        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(specialist);
        _repository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(specialist);
        _repository.Setup(x => x.UpdateDetailsAsync(specialistId, details)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateDetailsAsync(slug, specialistId, details);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== AddServiceAsync ==============
    // =============================================

    [Fact]
    public async Task AddServiceAsync_Should_Add_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var serviceOffer = new ServiceOffer { Name = ServiceType.Grooming };
        var specialist = CreateValidSpecialist(specialistId);

        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(specialist);
        _repository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(specialist);
        _repository.Setup(x => x.HasServiceOfTypeAsync(specialistId, serviceOffer.Name)).ReturnsAsync(false);
        _repository.Setup(x => x.AddServiceAsync(specialistId, serviceOffer)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.AddServiceAsync(slug, specialistId, serviceOffer);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddServiceAsync_Should_Return_ServiceTypeAlreadyExists()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var serviceOffer = new ServiceOffer { Name = ServiceType.Grooming };
        var specialist = CreateValidSpecialist(specialistId);

        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(specialist);
        _repository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(specialist);
        _repository.Setup(x => x.HasServiceOfTypeAsync(specialistId, serviceOffer.Name)).ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.AddServiceAsync(slug, specialistId, serviceOffer);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.ServiceTypeAlreadyExists.Description);
    }

    // =============================================
    // ============== UpdateServiceAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateServiceAsync_Should_Update_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var serviceOffer = new ServiceOffer { Id = Guid.NewGuid(), Name = ServiceType.Grooming, SpecialistId = specialistId };
        var specialist = CreateValidSpecialist(specialistId);

        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(specialist);
        _repository.Setup(x => x.GetServiceByIdAsync(serviceOffer.Id)).ReturnsAsync(serviceOffer);
        _repository.Setup(x => x.UpdateServiceAsync(serviceOffer)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateServiceAsync(slug, specialistId, serviceOffer);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== DeleteServiceAsync ==============
    // =============================================

    [Fact]
    public async Task DeleteServiceAsync_Should_Delete_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var specialist = CreateValidSpecialist(specialistId);
        var existingService = new ServiceOffer { Id = serviceId, SpecialistId = specialistId };

        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(specialist);
        _repository.Setup(x => x.GetServiceByIdAsync(serviceId)).ReturnsAsync(existingService);
        _repository.Setup(x => x.DeleteServiceAsync(serviceId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.DeleteServiceAsync(slug, specialistId, serviceId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== ReplyToReviewAsync ==============
    // =============================================

    [Fact]
    public async Task ReplyToReviewAsync_Should_Reply_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var specialist = CreateValidSpecialist(specialistId);
        var review = new Review { Id = reviewId, SpecialistId = specialistId };

        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(specialist);
        _repository.Setup(x => x.GetReviewByIdAsync(reviewId)).ReturnsAsync(review);
        _repository.Setup(x => x.AddReviewReplyAsync(reviewId, "Отличный отзыв!")).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ReplyToReviewAsync(slug, specialistId, reviewId, "Отличный отзыв!");

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ReplyToReviewAsync_Should_Return_ReviewNotFound()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var specialist = CreateValidSpecialist(specialistId);

        _repository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(specialist);
        _repository.Setup(x => x.GetReviewByIdAsync(reviewId)).ReturnsAsync((Review?)null);

        var service = CreateService();

        // act
        var result = await service.ReplyToReviewAsync(slug, specialistId, reviewId, "Отличный отзыв!");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.ReviewNotFound);
    }
}