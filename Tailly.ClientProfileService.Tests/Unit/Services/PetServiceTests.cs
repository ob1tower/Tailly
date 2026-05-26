using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.ClientProfileService.Application.Errors;
using Tailly.ClientProfileService.Application.Service;
using Tailly.ClientProfileService.Application.Service.Interfaces;
using Tailly.ClientProfileService.Core.Common;
using Tailly.ClientProfileService.Core.Enums;
using Tailly.ClientProfileService.Core.Models;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Tests.Unit.Services;

/// <summary>
/// Unit tests for PetService.
/// Covers:
/// - successful GetByUserAsync when profile and pets exist
/// - GetByUserAsync returns ProfileNotFound
/// - successful GetByIdAsync when pet belongs to user
/// - GetByIdAsync returns ProfileNotFound / PetNotFound / AccessDenied
/// - successful CreateAsync
/// - CreateAsync returns ProfileNotFound
/// - successful UpdateAsync
/// - UpdateAsync returns ProfileNotFound / PetNotFound / AccessDenied
/// - successful DeleteAsync
/// - DeleteAsync returns ProfileNotFound / PetNotFound / AccessDenied
/// </summary>
public class PetServiceTests
{
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly Mock<IClientProfileRepository> _profileRepository = new();
    private readonly Mock<ILogger<PetService>> _logger = new();

    private PetService CreateService()
    {
        return new PetService(
            _petRepository.Object,
            _profileRepository.Object,
            _logger.Object
        );
    }

    private ClientProfile CreateValidProfile(Guid userId)
    {
        return new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "Иван",
            LastName = "Иванов"
        };
    }

    private Pet CreateValidPet(Guid clientId)
    {
        return new Pet
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Name = "Барсик",
            Type = PetType.Cat,
            BreedId = Guid.NewGuid(),
            AgeYears = 3,
            Gender = PetGender.Male
        };
    }

    // =============================================
    // ============== GetByUserAsync ==============
    // =============================================

    [Fact]
    public async Task GetByUserAsync_Should_Return_Pets_When_Profile_Exists()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var pets = new List<Pet> { CreateValidPet(profile.Id) };

        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByClientIdAsync(profile.Id)).ReturnsAsync(pets);

        var service = CreateService();

        // act
        var result = await service.GetByUserAsync(userId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByUserAsync_Should_Return_ProfileNotFound_When_Profile_Does_Not_Exist()
    {
        // arrange
        var userId = Guid.NewGuid();
        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((ClientProfile?)null);

        var service = CreateService();

        // act
        var result = await service.GetByUserAsync(userId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.ProfileNotFound);
    }

    // =============================================
    // ============== GetByIdAsync ==============
    // =============================================

    [Fact]
    public async Task GetByIdAsync_Should_Return_Pet_When_Pet_Belongs_To_User()
    {
        // arrange
        var userId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var pet = CreateValidPet(profile.Id);

        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(petId)).ReturnsAsync(pet);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(userId, petId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(pet);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_ProfileNotFound()
    {
        // arrange
        _profileRepository.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((ClientProfile?)null);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.ProfileNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_PetNotFound()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Pet?)null);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(userId, Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.PetNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_AccessDenied_When_Pet_Belongs_To_Other_User()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var pet = CreateValidPet(Guid.NewGuid());

        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(pet.Id)).ReturnsAsync(pet);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(userId, pet.Id);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.AccessDenied);
    }

    // =============================================
    // ============== CreateAsync ==============
    // =============================================

    [Fact]
    public async Task CreateAsync_Should_Create_Pet_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var pet = new Pet
        {
            Name = "Мурзик",
            Type = PetType.Cat
        };

        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.AddAsync(It.IsAny<Pet>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(userId, pet);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ClientId.Should().Be(profile.Id);
        _petRepository.Verify(x => x.AddAsync(It.IsAny<Pet>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_ProfileNotFound()
    {
        // arrange
        _profileRepository.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((ClientProfile?)null);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(Guid.NewGuid(), new Pet());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.ProfileNotFound);
    }

    // =============================================
    // ============== UpdateAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateAsync_Should_Update_Pet_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var existingPet = CreateValidPet(profile.Id);
        var updatePet = new Pet
        {
            Id = existingPet.Id,
            Name = "Барсик Новый",
            AgeYears = 4,
            Type = PetType.Cat
        };

        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(existingPet.Id)).ReturnsAsync(existingPet);
        _petRepository.Setup(x => x.UpdateAsync(It.IsAny<Pet>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(userId, updatePet);

        // assert
        result.IsSuccess.Should().BeTrue();
        _petRepository.Verify(x => x.UpdateAsync(It.IsAny<Pet>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_ProfileNotFound()
    {
        // arrange
        _profileRepository.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((ClientProfile?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(Guid.NewGuid(), new Pet());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.ProfileNotFound.Description);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_PetNotFound()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Pet?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(userId, new Pet { Id = Guid.NewGuid() });

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.PetNotFound.Description);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_AccessDenied_When_Pet_Belongs_To_Other_User()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var otherPet = CreateValidPet(Guid.NewGuid());

        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(otherPet.Id)).ReturnsAsync(otherPet);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(userId, otherPet);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.AccessDenied.Description);
    }

    // =============================================
    // ============== DeleteAsync ==============
    // =============================================

    [Fact]
    public async Task DeleteAsync_Should_Delete_Pet_Successfully()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var pet = CreateValidPet(profile.Id);

        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(pet.Id)).ReturnsAsync(pet);
        _petRepository.Setup(x => x.DeleteAsync(pet.Id)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(userId, pet.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
        _petRepository.Verify(x => x.DeleteAsync(pet.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_ProfileNotFound()
    {
        // arrange
        _profileRepository.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((ClientProfile?)null);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.ProfileNotFound.Description);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_PetNotFound()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Pet?)null);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(userId, Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.PetNotFound.Description);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_AccessDenied_When_Pet_Belongs_To_Other_User()
    {
        // arrange
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var otherPet = CreateValidPet(Guid.NewGuid());

        _profileRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _petRepository.Setup(x => x.GetByIdAsync(otherPet.Id)).ReturnsAsync(otherPet);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(userId, otherPet.Id);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientProfileErrors.AccessDenied.Description);
    }
}