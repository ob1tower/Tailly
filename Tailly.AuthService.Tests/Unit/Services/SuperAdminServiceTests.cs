using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.SuperAdmin;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for SuperAdminService.
/// Covers:
/// - successful GetAllAsync with pagination
/// - CreateAsync success (new admin + email sent)
/// - CreateAsync returns EmailAlreadyExists
/// - DeleteAsync success
/// - DeleteAsync returns UserNotFound
/// - UpdateAsync success
/// - UpdateAsync returns UserNotFound
/// - UpdateBlockStatusAsync successfully blocks admin
/// - UpdateBlockStatusAsync returns UserNotFound
/// - ClearPasswordAttemptsLockAsync success
/// - GetPasswordRecoveryAsync returns requests
/// - ProcessPasswordRecoveryAsync success (reset + email)
/// - ProcessPasswordRecoveryAsync returns RequestAlreadyProcessed
/// </summary>
public class SuperAdminServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IAdminProfileRepository> _adminProfileRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IAdminPasswordRecoveryRepository> _passwordRecoveryRepository = new();
    private readonly Mock<IPasswordHashingService> _passwordHasher = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<ILogger<SuperAdminService>> _logger = new();

    private SuperAdminService CreateService()
    {
        return new SuperAdminService(
            _usersRepository.Object,
            _adminProfileRepository.Object,
            _refreshTokenRepository.Object,
            _passwordRecoveryRepository.Object,
            _passwordHasher.Object,
            _publishEndpoint.Object,
            _logger.Object
        );
    }

    private User CreateValidUser(RoleType role = RoleType.Admin)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            UserRoles = [new UserRole { Role = role }]
        };
    }

    // =============================================
    // ============== GetAllAsync ==============
    // =============================================

    [Fact]
    public async Task GetAllAsync_Should_Return_Admins_With_Pagination()
    {
        // arrange
        var admins = new List<AdminUserWithProfile>
        {
            new()
            {
                User = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "admin1@test.com",
                    FirstName = "Admin",
                    LastName = "One",
                    CreatedAt = DateTime.UtcNow
                },
                Role = new UserRoleEntity
                {
                    RoleId = (int)RoleType.Admin
                },
                Profile = null
            },
            new()
            {
                User = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "admin2@test.com",
                    FirstName = "Admin",
                    LastName = "Two",
                    CreatedAt = DateTime.UtcNow
                },
                Role = new UserRoleEntity
                {
                    RoleId = (int)RoleType.Admin
                },
                Profile = null
            }
        };

        _usersRepository
            .Setup(x => x.GetAdminsWithProfilesAsync(1, 10))
            .Returns(Task.FromResult(admins));

        _usersRepository
            .Setup(x => x.CountAdminsAsync())
            .ReturnsAsync(2);

        var service = CreateService();

        // act
        var result = await service.GetAllAsync(1, 10);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Total.Should().Be(2);
    }

    // =============================================
    // ============== CreateAsync ==============
    // =============================================

    [Fact]
    public async Task CreateAsync_Should_Create_New_Admin_And_Send_Email()
    {
        // arrange
        _usersRepository.Setup(x => x.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _usersRepository.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _usersRepository.Setup(x => x.AddRoleAsync(It.IsAny<Guid>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        _adminProfileRepository.Setup(x => x.AddAsync(It.IsAny<AdminProfileEntity>())).Returns(Task.CompletedTask);
        _passwordHasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed");

        var service = CreateService();

        // act
        var result = await service.CreateAsync("new@admin.com", "New", "Admin", null, DateTime.UtcNow.AddYears(-30), null, AdminDepartment.Administration);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.admin.Email.Should().Be("new@admin.com");
        _publishEndpoint.Verify(x => x.Publish(It.IsAny<SendEmailMessage>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_EmailAlreadyExists()
    {
        // arrange
        _usersRepository.Setup(x => x.ExistsAsync("existing@admin.com")).ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.CreateAsync("existing@admin.com", "Test", "User", null, DateTime.UtcNow.AddYears(-30), null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.EmailAlreadyExists);
    }

    // =============================================
    // ============== DeleteAsync ==============
    // =============================================

    [Fact]
    public async Task DeleteAsync_Should_Delete_Admin_Successfully()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetAdminByAdminIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _adminProfileRepository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(new AdminProfileEntity());
        _adminProfileRepository.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _usersRepository.Setup(x => x.DeleteUserRolesAsync(user.Id)).Returns(Task.CompletedTask);
        _usersRepository.Setup(x => x.DeleteUserAsync(user.Id)).Returns(Task.CompletedTask);
        _refreshTokenRepository.Setup(x => x.InvalidateAllAsync(user.Id)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(Guid.NewGuid());

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_UserNotFound()
    {
        // arrange
        _usersRepository.Setup(x => x.GetAdminByAdminIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.DeleteAsync(Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    // =============================================
    // ============== UpdateAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateAsync_Should_Update_Admin_Successfully()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetAdminByAdminIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _usersRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _adminProfileRepository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(new AdminProfileEntity());
        _adminProfileRepository.Setup(x => x.UpdateAsync(It.IsAny<AdminProfileEntity>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(Guid.NewGuid(), "Updated", "Admin", null, null, null, null);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_UserNotFound()
    {
        // arrange
        _usersRepository.Setup(x => x.GetAdminByAdminIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateAsync(Guid.NewGuid(), "John", "Doe", null, null, null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    // =============================================
    // ============== UpdateBlockStatusAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateBlockStatusAsync_Should_Block_Admin_Successfully()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetAdminByAdminIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _usersRepository.Setup(x => x.PatchUserRoleBlockAsync(It.IsAny<Guid>(), RoleType.Admin, true, false, null, null))
                        .ReturnsAsync(1);
        _refreshTokenRepository.Setup(x => x.InvalidateAllAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.UpdateBlockStatusAsync(Guid.NewGuid(), true, "Test reason");

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateBlockStatusAsync_Should_Return_UserNotFound()
    {
        // arrange
        _usersRepository.Setup(x => x.GetAdminByAdminIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateBlockStatusAsync(Guid.NewGuid(), true);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    // =============================================
    // ============== ClearPasswordAttemptsLockAsync ==============
    // =============================================

    [Fact]
    public async Task ClearPasswordAttemptsLockAsync_Should_Clear_Lock()
    {
        // arrange
        var user = CreateValidUser();
        var profile = new AdminProfileEntity { UserId = user.Id };
        _usersRepository.Setup(x => x.GetAdminByAdminIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _adminProfileRepository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(profile);
        _adminProfileRepository.Setup(x => x.UpdateAsync(It.IsAny<AdminProfileEntity>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ClearPasswordAttemptsLockAsync(Guid.NewGuid());

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== GetPasswordRecoveryAsync ==============
    // =============================================

    [Fact]
    public async Task GetPasswordRecoveryAsync_Should_Return_Requests()
    {
        // arrange
        var requests = new List<AdminPasswordRecovery> { new() };
        _passwordRecoveryRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

        var service = CreateService();

        // act
        var result = await service.GetPasswordRecoveryAsync();

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // =============================================
    // ============== ProcessPasswordRecoveryAsync ==============
    // =============================================

    [Fact]
    public async Task ProcessPasswordRecoveryAsync_Should_Process_Request_Successfully()
    {
        // arrange
        var request = new AdminPasswordRecovery { Id = Guid.NewGuid(), Email = "test@admin.com", Status = AdminPasswordRecoveryStatus.Pending };
        _passwordRecoveryRepository.Setup(x => x.GetByIdAsync(request.Id)).ReturnsAsync(request);
        _usersRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(CreateValidUser());
        _passwordHasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed");
        _usersRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _refreshTokenRepository.Setup(x => x.InvalidateAllAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _passwordRecoveryRepository.Setup(x => x.UpdateAsync(It.IsAny<AdminPasswordRecovery>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ProcessPasswordRecoveryAsync(request.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpoint.Verify(x => x.Publish(It.IsAny<SendEmailMessage>(), default), Times.Once);
    }

    [Fact]
    public async Task ProcessPasswordRecoveryAsync_Should_Return_RequestAlreadyProcessed()
    {
        // arrange
        var request = new AdminPasswordRecovery { Id = Guid.NewGuid(), Status = AdminPasswordRecoveryStatus.Processed };
        _passwordRecoveryRepository.Setup(x => x.GetByIdAsync(request.Id)).ReturnsAsync(request);

        var service = CreateService();

        // act
        var result = await service.ProcessPasswordRecoveryAsync(request.Id);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.RequestAlreadyProcessed);
    }
}