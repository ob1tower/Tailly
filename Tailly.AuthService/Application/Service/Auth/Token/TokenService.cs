using CSharpFunctionalExtensions;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Helpers;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.Auth.Token;

public class TokenService : ITokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IRefreshTokenRepository refreshTokenRepository,
                        IUsersRepository usersRepository,
                        IJwtTokenService jwtService,
                        IRefreshTokenService refreshTokenService,
                        ILogger<TokenService> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _usersRepository = usersRepository;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResult, Error>> RefreshTokenAsync(string refreshToken)
    {
        var hashedToken = _refreshTokenService.HashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Refresh failed. Token not found.");
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRefreshToken);
        }

        if (!storedToken.IsActive)
        {
            if (storedToken.Revoked != null)
            {
                _logger.LogWarning("Token reuse detected. Invalidating all tokens for user {UserId}", storedToken.UserId);
                await _refreshTokenRepository.InvalidateAllAsync(storedToken.UserId);
            }
            else
            {
                _logger.LogWarning("Refresh failed. Token expired for user {UserId}", storedToken.UserId);
            }
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRefreshToken);
        }

        var user = await _usersRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
        {
            _logger.LogWarning("Refresh failed. User not found for token {UserId}", storedToken.UserId);
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRefreshToken);
        }

        if (!SessionRole.TryResolveSessionRole(storedToken, user, out var sessionRole))
        {
            _logger.LogWarning("Refresh failed. Cannot resolve role for user {UserId}", user.Id);
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRefreshToken);
        }

        var roleEntity = user.GetRole(sessionRole); 
        if (roleEntity == null)
        {
            _logger.LogWarning("Refresh failed. Role {Role} not found for user {UserId}", sessionRole, user.Id);
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRefreshToken);
        }

        if (roleEntity.IsPendingDeletion)
        {
            _logger.LogWarning("Refresh failed. Role is pending deletion for user {UserId}", user.Id);
            return Result.Failure<AuthResult, Error>(AuthErrors.AccountPendingDeletion); // или InvalidRefreshToken
        }

        if (roleEntity.IsEffectivelyBlocked)
        {
            _logger.LogWarning("Refresh failed. Role is blocked for user {UserId}", user.Id);
            return Result.Failure<AuthResult, Error>(AuthErrors.AccountBlocked);
        }

        var (accessToken, accessExpires) = await _jwtService.CreateAccessTokenAsync(new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            UserRoles = new List<UserRoleEntity>
            {
                new UserRoleEntity { UserId = user.Id, RoleId = (int)sessionRole }
            }
        });

        var (rawRefreshToken, hashedRefreshToken) = _refreshTokenService.GenerateToken();
        var refreshExpires = _refreshTokenService.GetRefreshTokenExpiryDate();

        await _refreshTokenRepository.InvalidateAsync(storedToken.TokenHash);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = hashedRefreshToken,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = refreshExpires,
            RoleId = (int)sessionRole
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);

        _logger.LogInformation("Token refreshed successfully for user {UserId} with role {Role}", user.Id, sessionRole);

        return Result.Success<AuthResult, Error>(new AuthResult
        {
            AccessToken = accessToken,
            AccessTokenExpires = accessExpires,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpires = newRefreshToken.Expires
        });
    }

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        var hashedToken = _refreshTokenService.HashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            _logger.LogWarning("Logout failed. Token not found or already invalid.");
            return Result.Failure(AuthErrors.InvalidRefreshToken.Description);
        }

        await _refreshTokenRepository.InvalidateAsync(hashedToken);
        _logger.LogInformation("User logged out successfully.");

        return Result.Success();
    }
}