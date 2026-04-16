using CSharpFunctionalExtensions;
using Tailly.AuthService.Application.Errors;
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
                _logger.LogWarning("Refresh token reuse detected for user {UserId}. All tokens invalidated.",
                    storedToken.UserId);

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
            _logger.LogWarning("Refresh failed. User not found for token.");
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRefreshToken);
        }

        var (accessToken, accessExpires) = await _jwtService.CreateAccessTokenAsync(new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            UserRoles = user.Roles.Select(r => new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = (int)r
            }).ToList()
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
            Expires = refreshExpires
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);

        var result = new AuthResult
        {
            AccessToken = accessToken,
            AccessTokenExpires = accessExpires,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpires = newRefreshToken.Expires
        };

        _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

        return Result.Success<AuthResult, Error>(result);
    }

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        var hashedToken = _refreshTokenService.HashToken(refreshToken);

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Logout failed. Token not found.");
            return Result.Failure(AuthErrors.InvalidRefreshToken.Description);
        }

        if (!storedToken.IsActive)
        {
            _logger.LogWarning("Logout failed. Token already revoked or expired.");
            return Result.Failure(AuthErrors.InvalidRefreshToken.Description);
        }

        await _refreshTokenRepository.InvalidateAsync(hashedToken);

        _logger.LogInformation("User logged out. Token revoked.");

        return Result.Success();
    }
}