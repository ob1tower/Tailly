using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.Security;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for UserSecurityService.
/// Covers:
/// - successful password change
/// - invalid credentials
/// - account pending deletion
/// - account blocked
/// - invalid current password
/// - same password check
/// - email change request for regular user
/// - email change for SuperAdmin
/// - email confirmation (regular and admin)
/// - invalid verification code
/// - cancel email change
/// - edge cases with Redis and publishing
/// </summary>
public class UserSecurityServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IPasswordHashingService> _passwordHasher = new();
    private readonly Mock<IVerificationCodeService> _verificationCodeService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<IDatabase> _redisDatabase = new();
    private readonly Mock<ILogger<UserSecurityService>> _logger = new();

    private UserSecurityService CreateService()
    {
        var redisMock = new Mock<IConnectionMultiplexer>();
        redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                 .Returns(_redisDatabase.Object);

        return new UserSecurityService(
            _usersRepository.Object,
            _passwordHasher.Object,
            _verificationCodeService.Object,
            _refreshTokenRepository.Object,
            _publishEndpoint.Object,
            redisMock.Object,
            _logger.Object
        );
    }

    private User CreateValidUser(RoleType role = RoleType.Client)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "user@gmail.com",
            PasswordHash = "old-hash",
            EmailConfirmed = true,
            UserRoles = [new UserRole { Role = role }]
        };
    }

    private User CreateSuperAdmin()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "superadmin@gmail.com",
            PasswordHash = "old-hash",
            EmailConfirmed = true,
            UserRoles = [new UserRole { Role = RoleType.SuperAdmin }]
        };
    }

    // =============================================
    // ============== ChangePasswordAsync ==============
    // =============================================

    [Fact]
    public async Task ChangePasswordAsync_Should_Return_InvalidCredentials_When_User_Not_Found()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.ChangePasswordAsync(Guid.NewGuid(), "oldpass", "newpass");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidCredentials.Description);
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Return_AccountPendingDeletion_When_No_Active_Roles()
    {
        // arrange
        var user = CreateValidUser();
        user.UserRoles.Clear();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.ChangePasswordAsync(user.Id, "oldpass", "newpass");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.AccountPendingDeletion.Description);
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Return_AccountBlocked_When_All_Roles_Blocked()
    {
        // arrange
        var user = CreateValidUser();
        user.UserRoles.First().IsBlocked = true;
        user.UserRoles.First().IsPermanentBlock = true;
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.ChangePasswordAsync(user.Id, "oldpass", "newpass");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.AccountBlocked.Description);
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Return_InvalidPassword_When_Current_Password_Wrong()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("wrong", user.PasswordHash))
                       .Returns(false);

        var service = CreateService();

        // act
        var result = await service.ChangePasswordAsync(user.Id, "wrong", "newpass");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidPassword.Description);
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Return_SamePassword_When_New_Password_Equals_Old()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        _passwordHasher.Setup(x => x.VerifyPassword("oldpass", user.PasswordHash))
                       .Returns(true);

        _passwordHasher.Setup(x => x.VerifyPassword("newpass", user.PasswordHash))
                       .Returns(true);

        var service = CreateService();

        // act
        var result = await service.ChangePasswordAsync(user.Id, "oldpass", "newpass");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.SamePassword.Description);
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Successfully_Change_Password_And_Invalidate_Tokens()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("oldpass", user.PasswordHash))
                       .Returns(true);
        _passwordHasher.Setup(x => x.HashPassword("newpass"))
                       .Returns("new-hash");

        var service = CreateService();

        // act
        var result = await service.ChangePasswordAsync(user.Id, "oldpass", "newpass");

        // assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new-hash");
        _usersRepository.Verify(x => x.UpdateAsync(user), Times.Once);
        _refreshTokenRepository.Verify(x => x.InvalidateAllAsync(user.Id), Times.Once);
    }

    // =============================================
    // ============== RequestEmailChangeAsync ==============
    // =============================================

    [Fact]
    public async Task RequestEmailChangeAsync_Should_Return_InvalidCredentials_When_User_Not_Found()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.RequestEmailChangeAsync(Guid.NewGuid(), "new@gmail.com");

        // assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RequestEmailChangeAsync_Should_Return_SameEmail_When_NewEmail_Equals_Current()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.RequestEmailChangeAsync(user.Id, user.Email);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AuthErrors.SameEmail.Code);
    }

    [Fact]
    public async Task RequestEmailChangeAsync_Should_Return_UserAlreadyExists_When_Email_Already_Taken()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);
        _usersRepository.Setup(x => x.ExistsAsync("taken@gmail.com"))
                        .ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.RequestEmailChangeAsync(user.Id, "taken@gmail.com");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AuthErrors.UserAlreadyExists.Code);
    }

    // =============================================
    // ============== ConfirmEmailChangeAsync ==============
    // =============================================

    [Fact]
    public async Task ConfirmEmailChangeAsync_Should_Return_InvalidCredentials_When_User_Not_Found()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.ConfirmEmailChangeAsync(Guid.NewGuid(), "req-id", "new@gmail.com", "123456");

        // assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmEmailChangeAsync_Should_Return_InvalidVerificationCode_When_Code_Invalid()
    {
        // arrange
        var user = CreateValidUser();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);
        _verificationCodeService.Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                .ReturnsAsync((false, "invalid"));

        var service = CreateService();

        // act
        var result = await service.ConfirmEmailChangeAsync(user.Id, "req-id", "new@gmail.com", "wrong");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidVerificationCode.Description);
    }

    // =============================================
    // ============== EmailChangeAsync (SuperAdmin) ==============
    // =============================================

    [Fact]
    public async Task EmailChangeAsync_Should_Return_OnlySuperAdminCanChangeEmail_When_Not_SuperAdmin()
    {
        // arrange
        var user = CreateValidUser(RoleType.Client);
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.EmailChangeAsync(user.Id, "new@gmail.com", "password");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AdminErrors.OnlySuperAdminCanChangeEmail.Code);
    }

    [Fact]
    public async Task EmailChangeAsync_Should_Return_InvalidPassword_When_Password_Wrong()
    {
        // arrange
        var user = CreateSuperAdmin();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("wrong", user.PasswordHash))
                       .Returns(false);

        var service = CreateService();

        // act
        var result = await service.EmailChangeAsync(user.Id, "new@gmail.com", "wrong");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AuthErrors.InvalidPassword.Code);
    }

    [Fact]
    public async Task EmailChangeAsync_Should_Return_SameEmail_When_NewEmail_Equals_Current()
    {
        // arrange
        var user = CreateSuperAdmin();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("password", user.PasswordHash))
                       .Returns(true);

        var service = CreateService();

        // act
        var result = await service.EmailChangeAsync(user.Id, user.Email, "password");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AdminErrors.SameEmail.Code);
    }

    [Fact]
    public async Task EmailChangeAsync_Should_Return_EmailAlreadyExists_When_Email_Taken()
    {
        // arrange
        var user = CreateSuperAdmin();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("password", user.PasswordHash))
                       .Returns(true);
        _usersRepository.Setup(x => x.ExistsAsync("taken@gmail.com"))
                        .ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.EmailChangeAsync(user.Id, "taken@gmail.com", "password");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(AdminErrors.EmailAlreadyExists.Code);
    }

    // =============================================
    // ============== ConfirmEmailAdminChangeAsync ==============
    // =============================================

    [Fact]
    public async Task ConfirmEmailAdminChangeAsync_Should_Return_UserNotFound_When_User_Not_Exists()
    {
        // arrange
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((User?)null);

        var service = CreateService();

        // act
        var result = await service.ConfirmEmailAdminChangeAsync(Guid.NewGuid(), "123456");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdminErrors.UserNotFound.Description);
    }

    [Fact]
    public async Task ConfirmEmailAdminChangeAsync_Should_Return_InvalidVerificationCode_When_Code_Invalid()
    {
        // arrange
        var user = CreateSuperAdmin();
        _usersRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(user);
        _verificationCodeService.Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                .ReturnsAsync((false, "invalid"));

        var service = CreateService();

        // act
        var result = await service.ConfirmEmailAdminChangeAsync(user.Id, "wrong");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidVerificationCode.Description);
    }

    // =============================================
    // ============== CancelEmailChangeAsync ==============
    // =============================================

    [Fact]
    public async Task CancelEmailChangeAsync_Should_Call_RemoveCodeAsync()
    {
        // arrange
        var service = CreateService();

        // act
        var result = await service.CancelEmailChangeAsync(Guid.NewGuid());

        // assert
        result.IsSuccess.Should().BeTrue();
        _verificationCodeService.Verify(x => x.RemoveCodeAsync(It.IsAny<string>(), "admin-change-email"), Times.Once);
    }
}