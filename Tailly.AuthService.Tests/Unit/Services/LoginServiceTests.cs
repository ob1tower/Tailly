using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.Login;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for LoginService.
/// Covers:
/// - successful login for different roles
/// - invalid credentials
/// - blocked accounts
/// - pending deletion
/// - email confirmation validation
/// - invalid role scenarios
/// - admin temporary lock logic
/// - failed password attempts
/// - expired block restoration
/// - refresh token creation
/// - admin last login update
/// </summary>
public class LoginServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordHashingService> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtService = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenService = new();
    private readonly Mock<IAdminProfileRepository> _adminProfileRepository = new();
    private readonly Mock<ILogger<LoginService>> _logger = new();

    private LoginService CreateService()
    {
        return new LoginService(
            _usersRepository.Object,
            _refreshTokenRepository.Object,
            _passwordHasher.Object,
            _jwtService.Object,
            _refreshTokenService.Object,
            _adminProfileRepository.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task Should_Throw_When_Email_Is_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var action = async () =>
            await service.LoginAsync(null!, "123", RoleType.Client);

        // assert
        await action.Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Should_Trim_And_Lower_Email()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository
            .Setup(x => x.GetByEmailAsync("test@gmail.com"))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        SetupSuccessTokenFlow();
        var service = CreateService();

        // act
        await service.LoginAsync(
            " TEST@GMAIL.COM ",
            "123456",
            RoleType.Client);

        // assert
        _usersRepository.Verify(
            x => x.GetByEmailAsync("test@gmail.com"),
            Times.Once);
    }

    [Fact]
    public async Task Should_Return_InvalidCredentials_When_User_Not_Found()
    {
        // arrange
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "test@gmail.com",
            "123456",
            RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.InvalidCredentials.Code);
    }

    [Fact]
    public async Task Should_Return_InvalidRole_When_User_Does_Not_Have_Role()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "test@gmail.com",
            "123456",
            RoleType.Specialist);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.InvalidRole.Code);
    }

    [Fact]
    public async Task Should_Return_AccountBlocked_When_User_Is_Blocked()
    {
        // arrange
        var user = CreateValidUser();
        user.UserRoles =
        [
            new UserRole
            {
                Role = RoleType.Client,
                IsBlocked = true,
                IsPermanentBlock = true
            }
        ];
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "test@gmail.com",
            "123456",
            RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.AccountBlocked.Code);
    }

    [Fact]
    public async Task Should_Return_AccountPendingDeletion_When_User_Pending_Deletion()
    {
        // arrange
        var user = CreateValidUser();
        user.UserRoles =
        [
            new UserRole
            {
                Role = RoleType.Client,
                SoftDeletedAt = DateTime.UtcNow,
                RestoreUntil = DateTime.UtcNow.AddDays(7)
            }
        ];
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "test@gmail.com",
            "123456",
            RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.AccountPendingDeletion.Code);
    }

    [Fact]
    public async Task Should_Return_EmailNotConfirmed_When_Email_Not_Confirmed()
    {
        // arrange
        var user = CreateValidUser();
        user.EmailConfirmed = false;
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "test@gmail.com",
            "123456",
            RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.EmailNotConfirmed.Code);
    }

    [Fact]
    public async Task Should_Return_InvalidCredentials_When_Password_Invalid()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "test@gmail.com",
            "wrong-password",
            RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.InvalidCredentials.Code);
    }

    [Fact]
    public async Task Should_Increment_Admin_Failed_Attempts()
    {
        // arrange
        var user = CreateAdminUser();
        var profile = new AdminProfileEntity
        {
            FailedPasswordAttempts = 1
        };
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _adminProfileRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(profile);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        var service = CreateService();

        // act
        await service.LoginAsync(
            "admin@gmail.com",
            "wrong",
            RoleType.Admin);

        // assert
        profile.FailedPasswordAttempts.Should().Be(2);
        _adminProfileRepository.Verify(
            x => x.UpdateAsync(profile),
            Times.Once);
    }

    [Fact]
    public async Task Should_Lock_Admin_After_5_Attempts()
    {
        // arrange
        var user = CreateAdminUser();
        var profile = new AdminProfileEntity
        {
            FailedPasswordAttempts = 4
        };
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _adminProfileRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(profile);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        var service = CreateService();

        // act
        await service.LoginAsync(
            "admin@gmail.com",
            "wrong",
            RoleType.Admin);

        // assert
        profile.FailedPasswordAttempts.Should().Be(5);
        profile.PasswordAttemptsLockUntil.Should()
            .NotBeNull();
    }

    [Fact]
    public async Task Should_Return_AccountTemporarilyLocked_When_Admin_Locked()
    {
        // arrange
        var user = CreateAdminUser();
        var profile = new AdminProfileEntity
        {
            PasswordAttemptsLockUntil = DateTime.UtcNow.AddMinutes(10)
        };
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _adminProfileRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(profile);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "admin@gmail.com",
            "123",
            RoleType.Admin);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.AccountTemporarilyLocked.Code);
    }

    [Fact]
    public async Task Should_Return_AccessDenied_When_Admin_Has_Multiple_Roles()
    {
        // arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@gmail.com",
            PasswordHash = "hash",
            EmailConfirmed = true,
            UserRoles =
            [
                new UserRole { Role = RoleType.Admin },
                new UserRole { Role = RoleType.Admin }
            ]
        };
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "admin@gmail.com",
            "123",
            RoleType.Admin);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.AccessDenied.Code);
    }

    [Fact]
    public async Task Should_Unblock_User_When_Block_Expired()
    {
        // arrange
        var user = CreateValidUser();
        user.UserRoles =
        [
            new UserRole
            {
                Role = RoleType.Client,
                IsBlocked = true,
                IsPermanentBlock = false,
                BlockedUntil = DateTime.UtcNow.AddMinutes(-5)
            }
        ];
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        SetupSuccessTokenFlow();
        var service = CreateService();

        // act
        await service.LoginAsync(
            "test@gmail.com",
            "123456",
            RoleType.Client);

        // assert
        user.UserRoles.First().IsBlocked.Should().BeFalse();
        _usersRepository.Verify(
            x => x.UpdateAsync(user),
            Times.Once);
    }

    [Fact]
    public async Task Should_Update_Admin_Last_Login()
    {
        // arrange
        var user = CreateAdminUser();
        var profile = new AdminProfileEntity();
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _adminProfileRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(profile);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        SetupSuccessTokenFlow();
        var service = CreateService();

        // act
        await service.LoginAsync(
            "admin@gmail.com",
            "123456",
            RoleType.Admin);

        // assert
        profile.LastLoginAt.Should().NotBeNull();
        _adminProfileRepository.Verify(
            x => x.UpdateAsync(profile),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_Login_Successfully()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        SetupSuccessTokenFlow();
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "test@gmail.com",
            "123456",
            RoleType.Client);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should()
            .Be("access-token");
        result.Value.RefreshToken.Should()
            .Be("refresh-token");
        result.Value.User.Email.Should()
            .Be("test@gmail.com");
        _refreshTokenRepository.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Return_InvalidRole_When_Requesting_Admin_For_NonAdmin_User()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "test@gmail.com",
            "123456",
            RoleType.Admin);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.InvalidRole.Code);
    }

    [Fact]
    public async Task Should_Return_AccountBlocked_When_Admin_Role_Is_Blocked()
    {
        // arrange
        var user = CreateAdminUser();
        user.UserRoles =
        [
            new UserRole
            {
                Role = RoleType.Admin,
                IsBlocked = true,
                IsPermanentBlock = true
            }
        ];
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "admin@gmail.com",
            "123456",
            RoleType.Admin);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.AccountBlocked.Code);
    }

    [Fact]
    public async Task Should_Return_InvalidCredentials_When_Admin_Wrong_Password_And_No_Profile()
    {
        // arrange
        var user = CreateAdminUser();
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _adminProfileRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((AdminProfileEntity?)null);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "admin@gmail.com",
            "wrong",
            RoleType.Admin);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.InvalidCredentials.Code);
        _adminProfileRepository.Verify(
            x => x.UpdateAsync(It.IsAny<AdminProfileEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_Login_Successfully_As_Admin_With_No_Profile()
    {
        // arrange
        var user = CreateAdminUser();
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _adminProfileRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((AdminProfileEntity?)null);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        SetupSuccessTokenFlow();
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "admin@gmail.com",
            "123456",
            RoleType.Admin);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        _adminProfileRepository.Verify(
            x => x.UpdateAsync(It.IsAny<AdminProfileEntity>()),
            Times.Never);
        _refreshTokenRepository.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Login_Successfully_As_Admin_When_Lock_Expired()
    {
        // arrange
        var user = CreateAdminUser();
        var profile = new AdminProfileEntity
        {
            PasswordAttemptsLockUntil = DateTime.UtcNow.AddMinutes(-10)
        };
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _adminProfileRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(profile);
        _passwordHasher
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        SetupSuccessTokenFlow();
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "admin@gmail.com",
            "123456",
            RoleType.Admin);

        // assert
        result.IsSuccess.Should().BeTrue();
        profile.LastLoginAt.Should().NotBeNull();
        _adminProfileRepository.Verify(
            x => x.UpdateAsync(profile),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_Return_InvalidCredentials_When_Email_Is_Empty()
    {
        // arrange
        _usersRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        var service = CreateService();

        // act
        var result = await service.LoginAsync(
            "",
            "123456",
            RoleType.Client);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.InvalidCredentials.Code);
    }

    private User CreateValidUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "test@gmail.com",
            PasswordHash = "hash",
            EmailConfirmed = true,
            UserRoles =
            [
                new UserRole
                {
                    Role = RoleType.Client
                }
            ]
        };
    }

    private User CreateAdminUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@gmail.com",
            PasswordHash = "hash",
            EmailConfirmed = true,
            UserRoles =
            [
                new UserRole
                {
                    Role = RoleType.Admin
                }
            ]
        };
    }

    private void SetupSuccessTokenFlow()
    {
        _jwtService
            .Setup(x => x.CreateAccessTokenAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((
                "access-token",
                DateTime.UtcNow.AddHours(1)));
        _refreshTokenService
            .Setup(x => x.GenerateToken())
            .Returns(("refresh-token", "hashed-refresh-token"));
        _refreshTokenService
            .Setup(x => x.GetRefreshTokenExpiryDate())
            .Returns(DateTime.UtcNow.AddDays(7));
    }
}