using CSharpFunctionalExtensions;
using Tailly.AuthService.Entities;
using Tailly.AuthService.Errors;
using Tailly.AuthService.Models;
using Tailly.AuthService.Repositories;
using Tailly.AuthService.Repositories.Interfaces;
using Tailly.AuthService.Service.Security;
using Tailly.AuthService.Service.Tokens;

namespace Tailly.AuthService.Service.Auth;

public class AuthService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUsersRepository usersRepository,
                       IRefreshTokenRepository refreshTokenRepository,
                       IPasswordHashingService passwordHasher,
                       IJwtTokenService jwtService,
                       IRefreshTokenService refreshTokenService, 
                       ILogger<AuthService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<Guid>> RegisterAsync(string email, string password,
                                                  int roleId)
    {
        var result = await _usersRepository.ExistsAsync(email, roleId);

        if (result)
        {
            _logger.LogWarning("Registration failed. User already exists: {Email}, role {RoleId}", email, roleId);
            return Result.Failure<Guid>(AuthErrors.UserAlreadyExists.Description);
        }

        var passwordHash = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };

        await _usersRepository.AddAsync(user);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return Result.Success(user.Id);
    }

    public async Task<Result<AuthResult>> LoginAsync(string email, string password, 
                                                     int roleId)
    {
        var user = await _usersRepository.GetByEmailAndRoleAsync(email, roleId);

        if (user == null)
        {
            _logger.LogWarning("Login failed. User not found: {Email}, role {RoleId}", email, roleId);
            return Result.Failure<AuthResult>(AuthErrors.InvalidCredentials.Description);
        }

        var validPassword = _passwordHasher.VerifyPassword(password, user.PasswordHash);

        if (!validPassword)
        {
            _logger.LogWarning("Login failed. Invalid password for {Email}", email);
            return Result.Failure<AuthResult>(AuthErrors.InvalidCredentials.Description);
        }

        var (accessToken, accessExpires) = await _jwtService.CreateAccessTokenAsync(new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            RoleId = user.RoleId
        });

        var (rawRefreshToken, hashedRefreshToken) = _refreshTokenService.GenerateToken();

        var refreshExpires = DateTime.UtcNow.AddDays(30);

        var result = new AuthResult
        {
            AccessToken = accessToken,
            AccessTokenExpires = accessExpires,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpires = refreshExpires
        };

        return Result.Success(result);
    }

    public async Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken)
    {
        var hashedToken = _refreshTokenService.HashToken(refreshToken);

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Refresh failed. Token not found");
            return Result.Failure<AuthResult>(AuthErrors.InvalidRefreshToken.Description);
        }

        if (storedToken.Expires < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh failed. Token expired");
            return Result.Failure<AuthResult>(AuthErrors.RefreshTokenExpired.Description);
        }

        var user = await _usersRepository.GetByIdAsync(storedToken.UserId);

        if (user == null)
        {
            _logger.LogWarning("Refresh failed. User not found");
            return Result.Failure<AuthResult>(AuthErrors.InvalidRefreshToken.Description);
        }

        var (accessToken, accessExpires) = await _jwtService.CreateAccessTokenAsync(new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            RoleId = user.RoleId
        });

        var (rawRefreshToken, hashedRefreshToken) = _refreshTokenService.GenerateToken();

        storedToken.Revoked = DateTime.UtcNow;

        await _refreshTokenRepository.InvalidateAsync(storedToken.TokenHash);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = hashedRefreshToken,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(30)
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);

        var result = new AuthResult
        {
            AccessToken = accessToken,
            AccessTokenExpires = accessExpires,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpires = newRefreshToken.Expires
        };

        return Result.Success(result);
    }

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        var hashedToken = _refreshTokenService.HashToken(refreshToken);

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Logout failed. Token not found");
            return Result.Failure(AuthErrors.InvalidRefreshToken.Description);
        }

        await _refreshTokenRepository.InvalidateAsync(hashedToken);

        _logger.LogInformation("User logged out. Token revoked");

        return Result.Success();
    }
}