using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using MockQueryable;
using MockQueryable.Moq;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Admin;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for AdminUserService.
/// Covers:
/// - successful GetAllAsync with pagination (no filters)
/// - GetAllAsync filtering by search term
/// - GetAllAsync filtering by role
/// - GetByIdAsync success for existing user and role
/// - GetByIdAsync returns UserNotFound
/// - GetByIdAsync returns UserRoleNotFound
/// - UpdateBlockStatusAsync returns UserNotFound
/// - UpdateBlockStatusAsync successfully blocks user + invalidates tokens + sends email
/// - UpdateBlockStatusAsync successfully unblocks user + invalidates tokens
/// - RestoreFromDeletionAsync returns UserNotFound
/// - RestoreFromDeletionAsync returns UserNotDeleted
/// - RestoreFromDeletionAsync successfully restores user
/// - UpdateProfileAsync returns UserNotFound
/// - UpdateProfileAsync successfully updates profile
/// - UpdateProfileAsync returns SpecialistOnlyField for non-specialist
/// - UpdateProfileAsync successfully updates specialist slug
/// </summary>
public class AdminUserServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<ILogger<AdminUserService>> _logger = new();

    private AdminUserService CreateService()
    {
        return new AdminUserService(
            _usersRepository.Object,
            _refreshTokenRepository.Object,
            _publishEndpoint.Object,
            _logger.Object
        );
    }

    private User CreateValidUser(RoleType role = RoleType.Client)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "user@gmail.com",
            FirstName = "Test",
            LastName = "User",
            UserRoles = [new UserRole { Role = role }]
        };
    }

    private UserEntity CreateValidUserEntity(RoleType role = RoleType.Client)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "user@gmail.com",
            FirstName = "Test",
            LastName = "User",
            UserRoles = [new UserRoleEntity { RoleId = (int)role, SoftDeletedAt = null }]
        };
    }

    // =============================================
    // ============== GetAllAsync ==============
    // =============================================
    [Fact]
    public async Task GetAllAsync_Should_Return_Users_With_Pagination_When_No_Filters()
    {
        // arrange
        var entity1 = CreateValidUserEntity();
        var entity2 = CreateValidUserEntity();

        var users = new List<UserEntity>
        {
            entity1,
            entity2
        };

        var mock = users.BuildMock();

        _usersRepository.Setup(x => x.Query())
            .Returns(mock);

        var service = CreateService();

        // act
        var result = await service.GetAllAsync(null, null, 1, 10);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter_By_Search_Term()
    {
        // arrange
        var entity = CreateValidUserEntity();
        entity.FirstName = "SearchTestUser";

        var users = new List<UserEntity>
        {
            entity
        };

        var mock = users.BuildMock();

        _usersRepository.Setup(x => x.Query())
            .Returns(mock);

        var service = CreateService();

        // act
        var result = await service.GetAllAsync("searchtest", null, 1, 10);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    // =============================================
    // ============== GetByIdAsync ==============
    // =============================================

    [Fact]
    public async Task GetByIdAsync_Should_Return_AdminUser_When_User_And_Role_Exist()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(user.Id))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(user.Id, "client");

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Role.Should().Be("client");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_UserNotFound_When_User_Does_Not_Exist()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(Guid.NewGuid(), "Client");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_UserRoleNotFound_When_Role_Not_Exist()
    {
        // arrange
        var user = CreateValidUser(RoleType.Client);
        _usersRepository.Setup(x => x.GetByIdAsync(user.Id))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.GetByIdAsync(user.Id, "Specialist");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserRoleNotFound);
    }

    // =============================================
    // ============== UpdateBlockStatusAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateBlockStatusAsync_Should_Return_UserNotFound_When_User_Does_Not_Exist()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateBlockStatusAsync(Guid.NewGuid(), RoleType.Client, true, false, null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    [Fact]
    public async Task UpdateBlockStatusAsync_Should_Block_User_And_Invalidate_Tokens_And_Send_Email()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.UpdateBlockStatusAsync(user.Id, RoleType.Client, true, false, DateTime.UtcNow.AddDays(1), "Test reason");

        // assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenRepository.Verify(x => x.InvalidateAllForUserAndRoleAsync(user.Id, (int)RoleType.Client), Times.Once);
        _publishEndpoint.Verify(x => x.Publish(It.IsAny<SendEmailMessage>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateBlockStatusAsync_Should_Unblock_User_And_Invalidate_Tokens()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.UpdateBlockStatusAsync(user.Id, RoleType.Client, false, false, null, null);

        // assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenRepository.Verify(x => x.InvalidateAllForUserAndRoleAsync(user.Id, (int)RoleType.Client), Times.Once);
    }

    // =============================================
    // ============== RestoreFromDeletionAsync ==============
    // =============================================

    [Fact]
    public async Task RestoreFromDeletionAsync_Should_Return_UserNotFound_When_User_Does_Not_Exist()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.RestoreFromDeletionAsync(Guid.NewGuid(), "Client");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    [Fact]
    public async Task RestoreFromDeletionAsync_Should_Return_UserNotDeleted_When_Role_Not_SoftDeleted()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.RestoreFromDeletionAsync(user.Id, "Client");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotDeleted);
    }

    [Fact]
    public async Task RestoreFromDeletionAsync_Should_Successfully_Restore_User()
    {
        // arrange
        var user = CreateValidUser();
        user.UserRoles.First().SoftDeletedAt = DateTime.UtcNow.AddDays(-5);
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.RestoreFromDeletionAsync(user.Id, "Client");

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== UpdateProfileAsync ==============
    // =============================================

    [Fact]
    public async Task UpdateProfileAsync_Should_Return_UserNotFound_When_User_Does_Not_Exist()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.UpdateProfileAsync(Guid.NewGuid(), "John", "Doe", null, null);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound);
    }

    [Fact]
    public async Task UpdateProfileAsync_Should_Update_Profile_Successfully()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.UpdateProfileAsync(user.Id, "NewFirst", "NewLast", "NewMiddle", null);

        // assert
        result.IsSuccess.Should().BeTrue();
        _usersRepository.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_Should_Return_SpecialistOnlyField_When_Trying_To_Set_Slug_For_NonSpecialist()
    {
        // arrange
        var user = CreateValidUser(RoleType.Client);
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.UpdateProfileAsync(user.Id, "John", "Doe", null, "some-slug");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.SpecialistOnlyField);
    }

    [Fact]
    public async Task UpdateProfileAsync_Should_Update_Specialist_Slug_Successfully()
    {
        // arrange
        var user = CreateValidUser(RoleType.Specialist);
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.UpdateProfileAsync(user.Id, "John", "Doe", null, "new-specialist-slug");

        // assert
        result.IsSuccess.Should().BeTrue();
        _usersRepository.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}