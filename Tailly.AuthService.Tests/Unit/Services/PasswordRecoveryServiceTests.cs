using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.PasswordRecovery;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for PasswordRecoveryService.
/// Covers:
/// - password recovery flow selection
/// - admin recovery requests
/// - blocked users
/// - verification codes
/// - password reset
/// - refresh token invalidation
/// </summary>
public class PasswordRecoveryServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IPasswordHashingService> _passwordHasherMock = new();
    private readonly Mock<IVerificationCodeService> _verificationCodeServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IAdminPasswordRecoveryRepository> _passwordRecoveryRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogger<PasswordRecoveryService>> _loggerMock = new();

    private readonly PasswordRecoveryService _sut;

    public PasswordRecoveryServiceTests()
    {
        _sut = new PasswordRecoveryService(
            _usersRepositoryMock.Object,
            _passwordHasherMock.Object,
            _verificationCodeServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _passwordRecoveryRepositoryMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task StartPasswordRecoveryAsync_Should_Throw_When_Email_Is_Null()
    {
        // Arrange
        var action = async () =>
            await _sut.StartPasswordRecoveryAsync(null!);

        // Act & Assert
        await action.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public async Task StartPasswordRecoveryAsync_Should_Return_DefaultFlow_When_User_Not_Found()
    {
        // Arrange
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.StartPasswordRecoveryAsync(
            "ghost@tailly.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Flow.Should().Be("default");

        _passwordRecoveryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AdminPasswordRecovery>()),
            Times.Never);
    }

    [Fact]
    public async Task StartPasswordRecoveryAsync_Should_Return_DefaultFlow_For_Client()
    {
        // Arrange
        var user = CreateUser(RoleType.Client);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.StartPasswordRecoveryAsync(
            user.Email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Flow.Should().Be("default");
    }

    [Fact]
    public async Task StartPasswordRecoveryAsync_Should_Create_Admin_Request()
    {
        // Arrange
        var user = CreateUser(RoleType.Admin);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _passwordRecoveryRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync([]);

        // Act
        var result = await _sut.StartPasswordRecoveryAsync(
            user.Email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Flow.Should().Be("admin");

        _passwordRecoveryRepositoryMock.Verify(
            x => x.AddAsync(It.Is<AdminPasswordRecovery>(r =>
                r.Email == user.Email &&
                r.Status == AdminPasswordRecoveryStatus.Pending)),
            Times.Once);

        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<SendEmailMessage>(m =>
                    m.To == user.Email &&
                    m.Purpose == "admin-password-recovery-request"),
                default),
            Times.Once);
    }

    [Fact]
    public async Task StartPasswordRecoveryAsync_Should_Return_AccountBlocked_When_Admin_Blocked()
    {
        // Arrange
        var user = CreateUser(
            RoleType.Admin,
            isBlocked: true,
            isPermanentBlock: true);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.StartPasswordRecoveryAsync(
            user.Email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should()
            .Be(AuthErrors.AccountBlocked.Description);
    }

    [Fact]
    public async Task StartPasswordRecoveryAsync_Should_Return_Error_When_Request_Already_Pending()
    {
        // Arrange
        var user = CreateUser(RoleType.Admin);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _passwordRecoveryRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync([
                new AdminPasswordRecovery
                {
                    Email = user.Email,
                    Status = AdminPasswordRecoveryStatus.Pending
                }
            ]);

        // Act
        var result = await _sut.StartPasswordRecoveryAsync(
            user.Email);

        // Assert
        result.IsFailure.Should().BeTrue();

        _passwordRecoveryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AdminPasswordRecovery>()),
            Times.Never);
    }

    [Fact]
    public async Task SendRecoveryCodeAsync_Should_Throw_When_Email_Is_Null()
    {
        // Arrange
        var action = async () =>
            await _sut.SendRecoveryCodeAsync(null!);

        // Act & Assert
        await action.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public async Task SendRecoveryCodeAsync_Should_Return_Success_When_User_Not_Found()
    {
        // Arrange
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.SendRecoveryCodeAsync(
            "ghost@tailly.com");

        // Assert
        result.IsSuccess.Should().BeTrue();

        _verificationCodeServiceMock.Verify(
            x => x.SetCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendRecoveryCodeAsync_Should_Return_AccountBlocked_When_User_Blocked()
    {
        // Arrange
        var user = CreateUser(
            RoleType.Client,
            isBlocked: true,
            isPermanentBlock: true);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.SendRecoveryCodeAsync(
            user.Email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should()
            .Be(AuthErrors.AccountBlocked.Description);
    }

    [Fact]
    public async Task SendRecoveryCodeAsync_Should_Send_Code()
    {
        // Arrange
        var user = CreateUser(RoleType.Client);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.SendRecoveryCodeAsync(
            user.Email);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _verificationCodeServiceMock.Verify(
            x => x.SetCodeAsync(
                user.Email,
                It.IsAny<string>(),
                "password-recovery"),
            Times.Once);

        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<SendEmailMessage>(m =>
                    m.To == user.Email &&
                    m.Purpose == "password-recovery"),
                default),
            Times.Once);
    }

    [Fact]
    public async Task VerifyRecoveryCodeAsync_Should_Return_Failure_When_Code_Invalid()
    {
        // Arrange
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "password-recovery",
                false))
            .ReturnsAsync((false, "invalid"));

        // Act
        var result = await _sut.VerifyRecoveryCodeAsync(
            "user@tailly.com",
            "wrong");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyRecoveryCodeAsync_Should_Return_Success_When_Code_Valid()
    {
        // Arrange
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "password-recovery",
                false))
            .ReturnsAsync((true, null));

        // Act
        var result = await _sut.VerifyRecoveryCodeAsync(
            "user@tailly.com",
            "123456");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_Should_Return_Failure_When_User_Not_Found()
    {
        // Arrange
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.ResetPasswordAsync(
            "ghost@tailly.com",
            "123456",
            "newpass");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_Should_Return_AccountBlocked_When_User_Blocked()
    {
        // Arrange
        var user = CreateUser(
            RoleType.Client,
            isBlocked: true,
            isPermanentBlock: true);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ResetPasswordAsync(
            user.Email,
            "123456",
            "newpass");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should()
            .Be(AuthErrors.AccountBlocked.Description);
    }

    [Fact]
    public async Task ResetPasswordAsync_Should_Return_Error_When_NewPassword_Equals_Current()
    {
        // Arrange
        var user = CreateUser(RoleType.Client);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "samepass",
                user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _sut.ResetPasswordAsync(
            user.Email,
            "123456",
            "samepass");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_Should_Return_Error_When_Code_Invalid()
    {
        // Arrange
        var user = CreateUser(RoleType.Client);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(false);

        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "password-recovery"))
            .ReturnsAsync((false, "invalid"));

        // Act
        var result = await _sut.ResetPasswordAsync(
            user.Email,
            "wrong-code",
            "newpass");

        // Assert
        result.IsFailure.Should().BeTrue();

        _usersRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_Should_UpdatePassword_And_InvalidateRefreshTokens()
    {
        // Arrange
        var user = CreateUser(RoleType.Client);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(false);

        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "password-recovery"))
            .ReturnsAsync((true, null));

        _passwordHasherMock
            .Setup(x => x.HashPassword("newpass"))
            .Returns("new-hash");

        // Act
        var result = await _sut.ResetPasswordAsync(
            user.Email,
            "123456",
            "newpass");

        // Assert
        result.IsSuccess.Should().BeTrue();

        user.PasswordHash.Should()
            .Be("new-hash");

        _usersRepositoryMock.Verify(
            x => x.UpdateAsync(user),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.InvalidateAllAsync(user.Id),
            Times.Once);
    }

    private static User CreateUser(
        RoleType role,
        bool emailConfirmed = true,
        bool isBlocked = false,
        bool isPermanentBlock = false,
        DateTime? blockedUntil = null,
        string email = "user@tailly.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "old-hash",
            EmailConfirmed = emailConfirmed,
            UserRoles =
            [
                new UserRole
                {
                    Role = role,
                    IsBlocked = isBlocked,
                    IsPermanentBlock = isPermanentBlock,
                    BlockedUntil = blockedUntil
                }
            ]
        };
    }
}