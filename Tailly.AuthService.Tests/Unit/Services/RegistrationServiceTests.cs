using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.Common;
using Tailly.AuthService.Application.Service.Auth.Login;
using Tailly.AuthService.Application.Service.Auth.Registration;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;
using Tailly.Contracts.Messages;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for RegistrationService.
/// Covers:
/// - registration start flow
/// - email normalization
/// - existing active client validation
/// - blocked client validation
/// - specialist registration flow
/// - soft deleted client restoration flow
/// - verification code validation
/// - registration token attach
/// - invalid verification token handling
/// - registration completion flow
/// - existing user role updates
/// - adding client role to specialist
/// - new client creation
/// - refresh token creation
/// - JWT token generation
/// - successful auth result return
/// - UserRegistered event publishing
/// - pending registration cleanup
/// </summary>
public class RegistrationServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IPasswordHashingService> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenService> _jwtServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly Mock<IVerificationCodeService> _verificationCodeServiceMock = new();
    private readonly Mock<IPendingRegistrationService> _pendingRegistrationServiceMock = new();
    private readonly Mock<ILoginService> _loginServiceMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogger<RegistrationService>> _loggerMock = new();
    private RegistrationService CreateService()
    {
        return new RegistrationService(
            _usersRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _verificationCodeServiceMock.Object,
            _pendingRegistrationServiceMock.Object,
            _loginServiceMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );
    }
    private User CreateUserWithRole(
        RoleType role,
        bool emailConfirmed = true,
        bool isBlocked = false,
        bool isPermanentBlock = false,
        DateTime? blockedUntil = null,
        bool isSoftDeleted = false,
        string email = "test@tailly.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashed-password",
            EmailConfirmed = emailConfirmed,
            FirstName = "Test",
            LastName = "User",
            UserRoles =
            [
                new UserRole
                {
                    Role = role,
                    IsBlocked = isBlocked,
                    IsPermanentBlock = isPermanentBlock,
                    BlockedUntil = blockedUntil,
                    SoftDeletedAt = isSoftDeleted
                        ? DateTime.UtcNow.AddDays(-1)
                        : null,
                    RestoreUntil = isSoftDeleted
                        ? DateTime.UtcNow.AddDays(7)
                        : null
                }
            ]
        };
    }

    private void SetupSuccessTokenFlow()
    {
        _jwtServiceMock
            .Setup(x => x.CreateAccessTokenAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((
                "access-token",
                DateTime.UtcNow.AddHours(1)));
        _refreshTokenServiceMock
            .Setup(x => x.GenerateToken())
            .Returns(("refresh-token", "hashed-refresh-token"));
        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenExpiryDate())
            .Returns(DateTime.UtcNow.AddDays(7));
        _refreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task StartRegisterAsync_Should_Throw_When_Email_Is_Null()
    {
        // arrange
        var service = CreateService();

        // act
        var action = async () =>
            await service.StartRegisterAsync(null!, "123456");

        // assert
        await action.Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StartRegisterAsync_Should_Trim_And_Lower_Email()
    {
        // arrange
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("test@gmail.com"))
            .ReturnsAsync((User?)null);
        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hash");
        _pendingRegistrationServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("reg-id");
        _verificationCodeServiceMock
            .Setup(x => x.SetCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(
                It.IsAny<SendEmailMessage>(),
                default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        await service.StartRegisterAsync(
            " TEST@GMAIL.COM ",
            "123456");

        // assert
        _usersRepositoryMock.Verify(
            x => x.GetByEmailAsync("test@gmail.com"),
            Times.Once);
    }

    [Fact]
    public async Task StartRegisterAsync_Should_Return_UserAlreadyExists_When_Active_Client_Exists()
    {
        // arrange
        var user = CreateUserWithRole(
            RoleType.Client,
            email: "existing@gmail.com");
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.StartRegisterAsync(
            "existing@gmail.com",
            "123456");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.UserAlreadyExists.Code);
    }

    [Fact]
    public async Task StartRegisterAsync_Should_Return_AccountBlocked_When_Client_Blocked()
    {
        // arrange
        var user = CreateUserWithRole(
            RoleType.Client,
            isBlocked: true,
            isPermanentBlock: true,
            email: "blocked@gmail.com");
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.StartRegisterAsync(
            "blocked@gmail.com",
            "123456");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.AccountBlocked.Code);
    }

    [Fact]
    public async Task StartRegisterAsync_Should_Proceed_When_Specialist_Exists()
    {
        // arrange
        var user = CreateUserWithRole(
            RoleType.Specialist,
            email: "specialist@gmail.com");
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("specialist@gmail.com"))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hash");
        _pendingRegistrationServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("reg-spec");
        _verificationCodeServiceMock
            .Setup(x => x.SetCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(
                It.IsAny<SendEmailMessage>(),
                default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.StartRegisterAsync(
            "specialist@gmail.com",
            "123456");

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should()
            .Be("reg-spec");
    }

    [Fact]
    public async Task StartRegisterAsync_Should_Proceed_When_Client_Is_SoftDeleted()
    {
        // arrange
        var user = CreateUserWithRole(
            RoleType.Client,
            isSoftDeleted: true,
            email: "deleted@gmail.com");
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("deleted@gmail.com"))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hash");
        _pendingRegistrationServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("reg-del");
        _verificationCodeServiceMock
            .Setup(x => x.SetCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(
                It.IsAny<SendEmailMessage>(),
                default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.StartRegisterAsync(
            "deleted@gmail.com",
            "123456");

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should()
            .Be("reg-del");
    }

    [Fact]
    public async Task StartRegisterAsync_Should_Succeed_For_New_User()
    {
        // arrange
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _passwordHasherMock
            .Setup(x => x.HashPassword("123456"))
            .Returns("hashed-password");
        _pendingRegistrationServiceMock
            .Setup(x => x.CreateAsync(
                "new@gmail.com",
                "hashed-password"))
            .ReturnsAsync("reg-id");
        _verificationCodeServiceMock
            .Setup(x => x.SetCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "register"))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(
                It.IsAny<SendEmailMessage>(),
                default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.StartRegisterAsync(
            "new@gmail.com",
            "123456");

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should()
            .Be("reg-id");
        _passwordHasherMock.Verify(
            x => x.HashPassword("123456"),
            Times.Once);
        _pendingRegistrationServiceMock.Verify(
            x => x.CreateAsync(
                "new@gmail.com",
                "hashed-password"),
            Times.Once);
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<SendEmailMessage>(m =>
                    m.To == "new@gmail.com" &&
                    m.Purpose == "register"),
                default),
            Times.Once);
    }

    [Fact]
    public async Task VerifyRegisterAsync_Should_Return_RegistrationNotFound()
    {
        // arrange
        _pendingRegistrationServiceMock
            .Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(((string, string)?)null);
        var service = CreateService();

        // act
        var result = await service.VerifyRegisterAsync(
            "bad-id",
            "123456");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.RegistrationNotFound.Code);
    }

    [Fact]
    public async Task VerifyRegisterAsync_Should_Return_InvalidVerificationCode()
    {
        // arrange
        _pendingRegistrationServiceMock
            .Setup(x => x.GetAsync("reg-id"))
            .ReturnsAsync(("test@gmail.com", "hash"));
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "register"))
            .ReturnsAsync((false, "invalid"));
        var service = CreateService();

        // act
        var result = await service.VerifyRegisterAsync(
            "reg-id",
            "wrong");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.InvalidVerificationCode.Code);
    }

    [Fact]
    public async Task VerifyRegisterAsync_Should_Succeed_And_Attach_Token()
    {
        // arrange
        _pendingRegistrationServiceMock
            .Setup(x => x.GetAsync("reg-id"))
            .ReturnsAsync(("test@gmail.com", "hash"));
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "register"))
            .ReturnsAsync((true, (string?)null));
        _verificationCodeServiceMock
            .Setup(x => x.SetCodeAsync(
                It.IsAny<string>(),
                "ok",
                "register-token"))
            .Returns(Task.CompletedTask);
        _pendingRegistrationServiceMock
            .Setup(x => x.AttachTokenAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.VerifyRegisterAsync(
            "reg-id",
            "123456");

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should()
            .NotBeNullOrWhiteSpace();
        _pendingRegistrationServiceMock.Verify(
            x => x.AttachTokenAsync(
                "reg-id",
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteRegisterAsync_Should_Return_InvalidVerificationToken_When_Token_Invalid()
    {
        // arrange
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), "ok", "register-token"))
            .ReturnsAsync((false, "invalid"));
        var service = CreateService();

        // act
        var result = await service.CompleteRegisterAsync("bad-token", "John", "Doe", null, null, "");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.InvalidVerificationToken.Code);
    }

    [Fact]
    public async Task CompleteRegisterAsync_Should_Return_RegistrationNotFound_When_Data_Not_Found_By_Token()
    {
        // arrange
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), "ok", "register-token"))
            .ReturnsAsync((true, (string?)null));
        _pendingRegistrationServiceMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(((string, string)?)null);
        var service = CreateService();

        // act
        var result = await service.CompleteRegisterAsync("valid-token", "John", "Doe", null, null, "");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.RegistrationNotFound.Code);
    }

    [Fact]
    public async Task CompleteRegisterAsync_Should_Return_AccountBlocked_When_Existing_Client_Is_Blocked()
    {
        // arrange
        var user = CreateUserWithRole(
            RoleType.Client,
            isPermanentBlock: true,
            email: "blocked@gmail.com");
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), "ok", "register-token"))
            .ReturnsAsync((true, (string?)null));
        _pendingRegistrationServiceMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(("blocked@gmail.com", "hash"));
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("blocked@gmail.com"))
            .ReturnsAsync(user);
        var service = CreateService();

        // act
        var result = await service.CompleteRegisterAsync("token-123", "John", "Doe", null, null, "");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be(AuthErrors.AccountBlocked.Code);
    }

    [Fact]
    public async Task CompleteRegisterAsync_Should_Restore_Deleted_Client_Role_And_Succeed()
    {
        // arrange
        var user = CreateUserWithRole(
            RoleType.Client,
            isSoftDeleted: true,
            email: "restore@gmail.com");
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), "ok", "register-token"))
            .ReturnsAsync((true, (string?)null));
        _pendingRegistrationServiceMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(("restore@gmail.com", "hash"));
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("restore@gmail.com"))
            .ReturnsAsync(user);
        _usersRepositoryMock
            .Setup(x => x.PatchUserRoleSoftDeleteAsync(user.Id, RoleType.Client, null, null))
            .ReturnsAsync(1);
        SetupSuccessTokenFlow();
        _pendingRegistrationServiceMock
            .Setup(x => x.RemoveByTokenAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<UserRegisteredMessage>(), default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.CompleteRegisterAsync("token-123", "John", "Doe", null, null, "");

        // assert
        result.IsSuccess.Should().BeTrue();
        _usersRepositoryMock.Verify(
            x => x.PatchUserRoleSoftDeleteAsync(user.Id, RoleType.Client, null, null),
            Times.Once);
    }

    [Fact]
    public async Task CompleteRegisterAsync_Should_Proceed_For_Active_Client()
    {
        // arrange
        var user = CreateUserWithRole(RoleType.Client, email: "active@gmail.com");
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), "ok", "register-token"))
            .ReturnsAsync((true, (string?)null));
        _pendingRegistrationServiceMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(("active@gmail.com", "hash"));
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("active@gmail.com"))
            .ReturnsAsync(user);
        SetupSuccessTokenFlow();
        _pendingRegistrationServiceMock
            .Setup(x => x.RemoveByTokenAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<UserRegisteredMessage>(), default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.CompleteRegisterAsync("token-123", "John", "Doe", null, null, "");

        // assert
        result.IsSuccess.Should().BeTrue();
        _usersRepositoryMock.Verify(
            x => x.PatchUserRoleSoftDeleteAsync(It.IsAny<Guid>(), It.IsAny<RoleType>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()),
            Times.Never);
        _usersRepositoryMock.Verify(
            x => x.AddRoleAsync(It.IsAny<Guid>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task CompleteRegisterAsync_Should_Add_Client_Role_To_Existing_Specialist()
    {
        // arrange
        var user = CreateUserWithRole(RoleType.Specialist, email: "spec@gmail.com");
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), "ok", "register-token"))
            .ReturnsAsync((true, (string?)null));
        _pendingRegistrationServiceMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(("spec@gmail.com", "hash"));
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("spec@gmail.com"))
            .ReturnsAsync(user);
        _usersRepositoryMock
            .Setup(x => x.AddRoleAsync(user.Id, (int)RoleType.Client))
            .Returns(Task.CompletedTask);
        SetupSuccessTokenFlow();
        _pendingRegistrationServiceMock
            .Setup(x => x.RemoveByTokenAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<UserRegisteredMessage>(), default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.CompleteRegisterAsync("token-123", "John", "Doe", null, null, "");

        // assert
        result.IsSuccess.Should().BeTrue();
        _usersRepositoryMock.Verify(
            x => x.AddRoleAsync(user.Id, (int)RoleType.Client),
            Times.Once);
    }

    [Fact]
    public async Task CompleteRegisterAsync_Should_Create_New_User_As_Client()
    {
        // arrange
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), "ok", "register-token"))
            .ReturnsAsync((true, (string?)null));
        _pendingRegistrationServiceMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(("newuser@gmail.com", "newhash"));
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("newuser@gmail.com"))
            .ReturnsAsync((User?)null);
        _usersRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _usersRepositoryMock
            .Setup(x => x.AddRoleAsync(It.IsAny<Guid>(), (int)RoleType.Client))
            .Returns(Task.CompletedTask);
        SetupSuccessTokenFlow();
        _pendingRegistrationServiceMock
            .Setup(x => x.RemoveByTokenAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<UserRegisteredMessage>(), default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.CompleteRegisterAsync(
            "token-123", "John", "Doe", "Middle", "CityName", "city-123");

        // assert
        result.IsSuccess.Should().BeTrue();
        _usersRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(u =>
                u.Email == "newuser@gmail.com" &&
                u.EmailConfirmed &&
                u.FirstName == "John" &&
                u.LastName == "Doe")),
            Times.Once);
        _usersRepositoryMock.Verify(
            x => x.AddRoleAsync(It.IsAny<Guid>(), (int)RoleType.Client),
            Times.Once);
        _publishEndpointMock.Verify(
            x => x.Publish(It.Is<UserRegisteredMessage>(m =>
                m.Email == "newuser@gmail.com" &&
                m.FirstName == "John" &&
                m.LastName == "Doe" &&
                m.MiddleName == "Middle" &&
                m.CityName == "CityName" &&
                m.CityId == "city-123"), default),
            Times.Once);
    }

    [Fact]
    public async Task CompleteRegisterAsync_Should_Succeed_And_Return_AuthResult_With_Tokens_And_Publish_Event()
    {
        // arrange
        var user = CreateUserWithRole(RoleType.Client, email: "complete@gmail.com");
        _verificationCodeServiceMock
            .Setup(x => x.VerifyCodeAsync(It.IsAny<string>(), "ok", "register-token"))
            .ReturnsAsync((true, (string?)null));
        _pendingRegistrationServiceMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(("complete@gmail.com", "hash"));
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("complete@gmail.com"))
            .ReturnsAsync(user);
        SetupSuccessTokenFlow();
        _pendingRegistrationServiceMock
            .Setup(x => x.RemoveByTokenAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<UserRegisteredMessage>(), default))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        // act
        var result = await service.CompleteRegisterAsync(
            "token-123", "John", "Doe", null, "SomeCity", "city-id-1");

        // assert
        result.IsSuccess.Should().BeTrue();
        var authResult = result.Value;
        authResult.AccessToken.Should().Be("access-token");
        authResult.RefreshToken.Should().Be("refresh-token");
        authResult.User.Email.Should().Be("complete@gmail.com");
        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Once);
        _publishEndpointMock.Verify(
            x => x.Publish(It.Is<UserRegisteredMessage>(m =>
                m.UserId == user.Id &&
                m.Email == "complete@gmail.com" &&
                m.FirstName == "John" &&
                m.LastName == "Doe" &&
                m.CityName == "SomeCity" &&
                m.CityId == "city-id-1"), default),
            Times.Once);
    }
}