using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.AccountDeletion;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Configurations.Options;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for AccountDeletionService.
/// Covers:
/// - successful account deletion request for Client and Specialist
/// - invalid role for deletion request
/// - user not found during deletion request
/// - account blocked during deletion request
/// - invalid password during deletion request
/// - successful account restoration
/// - invalid or expired restoration token
/// - user not found during restoration
/// </summary>
public class AccountDeletionServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IPasswordHashingService> _passwordHasher = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IAccountDeletionTokenRepository> _tokenRepository = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<IOptions<FrontendOptions>> _frontendOptions = new();
    private readonly Mock<ILogger<AccountDeletionService>> _logger = new(); // хотя не используется напрямую

    private AccountDeletionService CreateService()
    {
        var frontendOptions = new FrontendOptions { BaseUrl = "https://tailly.ru" };
        _frontendOptions.Setup(x => x.Value).Returns(frontendOptions);

        return new AccountDeletionService(
            _usersRepository.Object,
            _passwordHasher.Object,
            _refreshTokenRepository.Object,
            _tokenRepository.Object,
            _publishEndpoint.Object,
            _frontendOptions.Object
        );
    }

    private User CreateValidUser(RoleType role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            PasswordHash = "hashed-password",
            UserRoles = [new UserRole { Role = role }]
        };
    }

    // =============================================
    // ============== RequestDeletionAsync ==============
    // =============================================

    [Fact]
    public async Task RequestDeletionAsync_Should_Successfully_Request_Deletion_For_Client()
    {
        // arrange
        var userId = Guid.NewGuid();
        var user = CreateValidUser(RoleType.Client);
        var userRole = user.UserRoles.First();
        userRole.SoftDeletedAt = null;
        userRole.IsBlocked = false;

        _usersRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("correctpass", user.PasswordHash)).Returns(true);
        _usersRepository.Setup(x => x.PatchUserRoleSoftDeleteAsync(userId, RoleType.Client, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                        .ReturnsAsync(1);
        _refreshTokenRepository.Setup(x => x.InvalidateAllForUserAndRoleAsync(userId, (int)RoleType.Client))
                               .Returns(Task.CompletedTask);
        _tokenRepository.Setup(x => x.AddAsync(It.IsAny<AccountDeletionToken>())).Returns(Task.CompletedTask);
        _publishEndpoint.Setup(x => x.Publish(It.IsAny<SendEmailMessage>(), default)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.RequestDeletionAsync(userId, "correctpass", RoleType.Client);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RequestDeletionAsync_Should_Return_InvalidRole_For_NonClientOrSpecialist()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.RequestDeletionAsync(Guid.NewGuid(), "pass", RoleType.Admin);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidRole.Description);
    }

    [Fact]
    public async Task RequestDeletionAsync_Should_Return_UserNotFound()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.RequestDeletionAsync(Guid.NewGuid(), "pass", RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.UserNotFound.Description);
    }

    [Fact]
    public async Task RequestDeletionAsync_Should_Return_AccountBlocked()
    {
        // arrange
        var user = CreateValidUser(RoleType.Client);
        user.UserRoles.First().IsBlocked = true;

        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.RequestDeletionAsync(Guid.NewGuid(), "pass", RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.AccountBlocked.Description);
    }

    [Fact]
    public async Task RequestDeletionAsync_Should_Return_InvalidPassword()
    {
        // arrange
        var user = CreateValidUser(RoleType.Client);
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var service = CreateService();

        // act
        var result = await service.RequestDeletionAsync(Guid.NewGuid(), "wrongpass", RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidPassword.Description);
    }

    // =============================================
    // ============== RestoreAsync ==============
    // =============================================

    [Fact]
    public async Task RestoreAsync_Should_Successfully_Restore_Account()
    {
        // arrange
        var token = "valid-restore-token";
        var userId = Guid.NewGuid();
        var role = RoleType.Client;

        var tokenModel = new AccountDeletionToken
        {
            Token = token,
            UserId = userId,
            RoleId = (int)role,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };

        var user = new User { Id = userId };

        _tokenRepository.Setup(x => x.GetAsync(token))
                        .ReturnsAsync(tokenModel);

        _usersRepository.Setup(x => x.GetByIdAsync(userId))
                        .ReturnsAsync(user);

        _usersRepository.Setup(x => x.PatchUserRoleSoftDeleteAsync(userId, role, null, null))
                        .ReturnsAsync(1);

        _tokenRepository.Setup(x => x.RemoveAsync(token))
                        .Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.RestoreAsync(token);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RestoreAsync_Should_Return_InvalidVerificationToken_When_Token_Invalid_Or_Expired()
    {
        // arrange
        _tokenRepository.Setup(x => x.GetAsync("bad-token")).ReturnsAsync((AccountDeletionToken?)null);

        var service = CreateService();

        // act
        var result = await service.RestoreAsync("bad-token");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidVerificationToken.Description);
    }

    [Fact]
    public async Task RestoreAsync_Should_Return_UserNotFound()
    {
        // arrange
        var tokenModel = new AccountDeletionToken { Token = "token", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _tokenRepository.Setup(x => x.GetAsync("token")).ReturnsAsync(tokenModel);
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.RestoreAsync("token");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.UserNotFound.Description);
    }
}