using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.Token;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Tests.Unit.Services;

/// <summary>
/// Unit tests for TokenService.
/// Covers:
/// - successful token refresh with new access and refresh tokens
/// - RefreshTokenAsync returns InvalidRefreshToken when token not found
/// - RefreshTokenAsync returns InvalidRefreshToken when token is inactive/expired
/// - RefreshTokenAsync returns AccountBlocked when role is blocked
/// - successful logout
/// - LogoutAsync returns InvalidRefreshToken when token not found or inactive
/// </summary>
public class TokenServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IJwtTokenService> _jwtService = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenService = new();
    private readonly Mock<ILogger<TokenService>> _logger = new();

    private TokenService CreateService()
    {
        return new TokenService(
            _refreshTokenRepository.Object,
            _usersRepository.Object,
            _jwtService.Object,
            _refreshTokenService.Object,
            _logger.Object
        );
    }

    private RefreshToken CreateValidActiveRefreshToken(Guid userId, RoleType role)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = "hashed-valid-token",
            UserId = userId,
            RoleId = (int)role,
            Created = DateTime.UtcNow.AddDays(-1),
            Expires = DateTime.UtcNow.AddDays(7),
            Revoked = null
        };
    }

    private User CreateValidUser(Guid id, RoleType role)
    {
        return new User
        {
            Id = id,
            Email = "user@test.com",
            UserRoles = [new UserRole { Role = role }]
        };
    }

    // =============================================
    // ============== RefreshTokenAsync ==============
    // =============================================

    [Fact]
    public async Task RefreshTokenAsync_Should_Successfully_Refresh_Tokens()
    {
        // arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid-refresh-token";
        var hashedToken = "hashed-valid-token";

        var storedToken = CreateValidActiveRefreshToken(userId, RoleType.Client);
        var user = CreateValidUser(userId, RoleType.Client);

        _refreshTokenService.Setup(x => x.HashToken(refreshToken)).Returns(hashedToken);
        _refreshTokenRepository.Setup(x => x.GetByTokenAsync(hashedToken)).ReturnsAsync(storedToken);
        _usersRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        _jwtService.Setup(x => x.CreateAccessTokenAsync(It.IsAny<UserEntity>()))
                   .ReturnsAsync(("new-access-token", DateTime.UtcNow.AddHours(1)));

        _refreshTokenService.Setup(x => x.GenerateToken()).Returns(("new-raw-token", "new-hashed-token"));
        _refreshTokenService.Setup(x => x.GetRefreshTokenExpiryDate()).Returns(DateTime.UtcNow.AddDays(7));

        _refreshTokenRepository.Setup(x => x.InvalidateAsync(hashedToken)).Returns(Task.CompletedTask);
        _refreshTokenRepository.Setup(x => x.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.RefreshTokenAsync(refreshToken);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-raw-token");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_InvalidRefreshToken_When_Token_Not_Found()
    {
        // arrange
        _refreshTokenService.Setup(x => x.HashToken(It.IsAny<string>())).Returns("hashed");
        _refreshTokenRepository.Setup(x => x.GetByTokenAsync("hashed")).ReturnsAsync((RefreshToken?)null);

        var service = CreateService();

        // act
        var result = await service.RefreshTokenAsync("invalid-token");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_InvalidRefreshToken_When_Token_Is_Inactive()
    {
        // arrange
        var userId = Guid.NewGuid();
        var storedToken = CreateValidActiveRefreshToken(userId, RoleType.Client);
        storedToken.Expires = DateTime.UtcNow.AddDays(-1);

        _refreshTokenService.Setup(x => x.HashToken(It.IsAny<string>())).Returns("hashed");
        _refreshTokenRepository.Setup(x => x.GetByTokenAsync("hashed")).ReturnsAsync(storedToken);

        var service = CreateService();

        // act
        var result = await service.RefreshTokenAsync("expired-token");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_AccountBlocked_When_Role_Is_Blocked()
    {
        // arrange
        var userId = Guid.NewGuid();
        var storedToken = CreateValidActiveRefreshToken(userId, RoleType.Client);
        var user = CreateValidUser(userId, RoleType.Client);
        user.UserRoles.First().IsBlocked = true;

        _refreshTokenService.Setup(x => x.HashToken(It.IsAny<string>())).Returns("hashed");
        _refreshTokenRepository.Setup(x => x.GetByTokenAsync("hashed")).ReturnsAsync(storedToken);
        _usersRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var service = CreateService();

        // act
        var result = await service.RefreshTokenAsync("valid-token");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.AccountBlocked);
    }

    // =============================================
    // ============== LogoutAsync ==============
    // =============================================

    [Fact]
    public async Task LogoutAsync_Should_Successfully_Logout()
    {
        // arrange
        var refreshToken = "valid-refresh-token";
        var hashedToken = "hashed-token";
        var storedToken = CreateValidActiveRefreshToken(Guid.NewGuid(), RoleType.Client);

        _refreshTokenService.Setup(x => x.HashToken(refreshToken)).Returns(hashedToken);
        _refreshTokenRepository.Setup(x => x.GetByTokenAsync(hashedToken)).ReturnsAsync(storedToken);
        _refreshTokenRepository.Setup(x => x.InvalidateAsync(hashedToken)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.LogoutAsync(refreshToken);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAsync_Should_Return_InvalidRefreshToken_When_Token_Not_Found_Or_Inactive()
    {
        // arrange
        _refreshTokenService.Setup(x => x.HashToken(It.IsAny<string>())).Returns("hashed");
        _refreshTokenRepository.Setup(x => x.GetByTokenAsync("hashed")).ReturnsAsync((RefreshToken?)null);

        var service = CreateService();

        // act
        var result = await service.LogoutAsync("invalid-token");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidRefreshToken.Description);
    }
}