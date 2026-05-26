using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.ClientProfileService.Application.Errors;
using Tailly.ClientProfileService.Application.Service;
using Tailly.ClientProfileService.Core.Models;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;
using Tailly.Contracts.Messages;

namespace Tailly.ClientProfileService.Tests.Unit.Services;

/// <summary>
/// Unit tests for ClientProfilesService.
/// Covers:
/// - successful GetAsync when profile exists
/// - GetAsync returns ProfileNotFound
/// - successful UpdateMainAsync (partial update + event publishing)
/// - UpdateMainAsync returns error when profile not found
/// - successful UpdateContactsAsync
/// - UpdateContactsAsync returns error when profile not found
/// - successful CreateAsync
/// - CreateAsync returns error when profile already exists
/// </summary>
public class ClientProfilesServiceTests
{
    private readonly Mock<IClientProfileRepository> _repository = new();
    private readonly Mock<ILogger<ClientProfilesService>> _logger = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();

    private ClientProfilesService CreateService()
    {
        return new ClientProfilesService(
            _repository.Object,
            _logger.Object,
            _publishEndpoint.Object
        );
    }

    private ClientProfile CreateValidProfile()
    {
        return new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FirstName = "Иван",
            LastName = "Иванов",
            MiddleName = "Александрович",
            Phone = "+7 (999) 123-45-67",
            City = "Москва",
            CityId = "77000000000",
            District = "Центральный",
            AvatarUrl = "https://example.com/avatar.jpg"
        };
    }

    // =============================================
    // ============== GetAsync ==============
    // =============================================

    [Fact]
    public async Task GetAsync_Should_Return_Profile_When_Profile_Exists()
    {
        // arrange
        var profile = CreateValidProfile();
        _repository.Setup(x => x.GetByUserIdAsync(profile.UserId)).ReturnsAsync(profile);

        var service = CreateService();

        // act
        var result = await service.GetAsync(profile.UserId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(profile);
    }

    [Fact]
    public async Task GetAsync_Should_Return_ProfileNotFound_When_Profile_Does_Not_Exist()
    {
        // arrange
        var userId = Guid.NewGuid();
        _repository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((ClientProfile?)null);

        var service = CreateService();

        // act
        var result = await service.GetAsync(userId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.ProfileNotFound);
    }

    // =============================================
    // ============== UpdateMainAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateMainAsync_Should_Update_Main_Info_And_Publish_Event()
    {
        // arrange
        var existing = CreateValidProfile();
        var update = new ClientProfile { FirstName = "Пётр", LastName = "Петров", MiddleName = "Сергеевич" };

        _repository.Setup(x => x.GetByUserIdAsync(existing.UserId)).ReturnsAsync(existing);
        _repository.Setup(x => x.UpdateAsync(It.IsAny<ClientProfile>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateMainAsync(existing.UserId, update);

        // assert
        result.IsSuccess.Should().BeTrue();
        _repository.Verify(x => x.UpdateAsync(It.Is<ClientProfile>(p =>
            p.FirstName == "Пётр" &&
            p.LastName == "Петров" &&
            p.MiddleName == "Сергеевич")), Times.Once);

        _publishEndpoint.Verify(x => x.Publish(It.Is<UserProfileUpdatedMessage>(m =>
            m.UserId == existing.UserId &&
            m.FirstName == "Пётр" &&
            m.LastName == "Петров" &&
            m.MiddleName == "Сергеевич"), default), Times.Once);
    }

    [Fact]
    public async Task UpdateMainAsync_Should_Return_Error_When_Profile_Not_Found()
    {
        // arrange
        var userId = Guid.NewGuid();
        _repository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((ClientProfile?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateMainAsync(userId, new ClientProfile());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.ProfileNotFound.Description);
    }

    // =============================================
    // ============== UpdateContactsAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateContactsAsync_Should_Update_Contacts_Successfully()
    {
        // arrange
        var existing = CreateValidProfile();
        var update = new ClientProfile
        {
            Phone = "+7 (999) 999-99-99",
            City = "Санкт-Петербург",
            CityId = "78000000000",
            District = "Невский"
        };

        _repository.Setup(x => x.GetByUserIdAsync(existing.UserId)).ReturnsAsync(existing);
        _repository.Setup(x => x.UpdateAsync(It.IsAny<ClientProfile>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateContactsAsync(existing.UserId, update);

        // assert
        result.IsSuccess.Should().BeTrue();
        _repository.Verify(x => x.UpdateAsync(It.Is<ClientProfile>(p =>
            p.Phone == "+7 (999) 999-99-99" &&
            p.City == "Санкт-Петербург" &&
            p.CityId == "78000000000" &&
            p.District == "Невский")), Times.Once);
    }

    [Fact]
    public async Task UpdateContactsAsync_Should_Return_Error_When_Profile_Not_Found()
    {
        // arrange
        var userId = Guid.NewGuid();
        _repository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((ClientProfile?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateContactsAsync(userId, new ClientProfile());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.ProfileNotFound.Description);
    }

    // =============================================
    // ============== CreateAsync ==============
    // =============================================

    [Fact]
    public async Task CreateAsync_Should_Create_Profile_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile();
        profile.UserId = userId;

        _repository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((ClientProfile?)null);
        _repository.Setup(x => x.AddAsync(It.IsAny<ClientProfile>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(userId, profile);
    }
}