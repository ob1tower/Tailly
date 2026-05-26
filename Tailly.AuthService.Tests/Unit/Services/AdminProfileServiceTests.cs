using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.AdminProfiles;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for AdminProfileService.
/// Covers:
/// - successful profile retrieval for regular admin
/// - super admin role detection
/// - user not found during profile retrieval
/// - successful profile update for regular admin
/// - birth date update allowed only for super admin
/// - user not found during profile update
/// - birth date update forbidden for regular admin
/// </summary>
public class AdminProfileServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IAdminProfileRepository> _adminProfileRepository = new();
    private readonly Mock<ILogger<AdminProfileService>> _logger = new();

    private AdminProfileService CreateService()
    {
        return new AdminProfileService(
            _usersRepository.Object,
            _adminProfileRepository.Object,
            _logger.Object
        );
    }

    private User CreateValidUser(RoleType role = RoleType.Admin)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@gmail.com",
            FirstName = "Admin",
            LastName = "User",
            UserRoles = [new UserRole { Role = role }]
        };
    }

    private AdminProfileEntity CreateValidAdminProfile()
    {
        return new AdminProfileEntity
        {
            UserId = Guid.NewGuid(),
            BirthDate = new DateTime(1990, 1, 1),
            Phone = "+7 (999) 123-45-67",
            Department = AdminDepartment.Administration
        };
    }

    // =============================================
    // ============== GetAsync ==============
    // =============================================

    [Fact]
    public async Task GetAsync_Should_Return_AdminProfile_When_User_Exists()
    {
        // arrange
        var user = CreateValidUser();
        var profile = CreateValidAdminProfile();

        _usersRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _adminProfileRepository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(profile);

        var service = CreateService();

        // act
        var result = await service.GetAsync(user.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Role.Should().Be("admin");
    }

    [Fact]
    public async Task GetAsync_Should_Return_SuperAdmin_Role_When_User_Is_SuperAdmin()
    {
        // arrange
        var user = CreateValidUser(RoleType.SuperAdmin);
        var profile = CreateValidAdminProfile();

        _usersRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _adminProfileRepository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(profile);

        var service = CreateService();

        // act
        var result = await service.GetAsync(user.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("super_admin");
    }

    [Fact]
    public async Task GetAsync_Should_Return_UserNotFound_When_User_Does_Not_Exist()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.GetAsync(Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    // =============================================
    // ============== UpdateAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateAsync_Should_Update_Regular_Admin_Profile_Successfully()
    {
        // arrange
        var user = CreateValidUser(RoleType.Admin);
        var profile = CreateValidAdminProfile();

        _usersRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _usersRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _adminProfileRepository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(profile);
        _adminProfileRepository.Setup(x => x.UpdateAsync(It.IsAny<AdminProfileEntity>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(user.Id, "NewFirst", "NewLast", null, null, "+7 (999) 999-99-99", AdminDepartment.HR);

        // assert
        result.IsSuccess.Should().BeTrue();
        _usersRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _adminProfileRepository.Verify(x => x.UpdateAsync(It.IsAny<AdminProfileEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Allow_BirthDate_For_SuperAdmin()
    {
        // arrange
        var user = CreateValidUser(RoleType.SuperAdmin);
        var profile = CreateValidAdminProfile();

        _usersRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _usersRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _adminProfileRepository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(profile);
        _adminProfileRepository.Setup(x => x.UpdateAsync(It.IsAny<AdminProfileEntity>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(user.Id, "NewFirst", "NewLast", null, DateTime.UtcNow.AddYears(-30), null, null);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_UserNotFound_When_User_Does_Not_Exist()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(Guid.NewGuid(), "John", "Doe", null, null, null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_BirthDateNotAllowed_For_Regular_Admin()
    {
        // arrange
        var user = CreateValidUser(RoleType.Admin);

        _usersRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(user.Id, "John", "Doe", null, DateTime.UtcNow.AddYears(-30), null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.BirthDateNotAllowed);
    }
}