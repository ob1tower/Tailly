using CSharpFunctionalExtensions;
using Tailly.AuthService.Entities;
using Tailly.AuthService.Enums;
using Tailly.AuthService.Errors;
using Tailly.AuthService.Models;
using Tailly.AuthService.Repositories.Interfaces;
using Tailly.AuthService.Service.Security;
using Tailly.AuthService.Service.Tokens;

namespace Tailly.AuthService.Service.Auth;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IUsersRepository usersRepository,
                       IRefreshTokenRepository refreshTokenRepository,
                       IPasswordHashingService passwordHasher,
                       IJwtTokenService jwtService,
                       IRefreshTokenService refreshTokenService,
                       ILogger<AuthenticationService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<Guid>> RegisterAsync(string email, string password)
    {
        email = email?.Trim().ToLowerInvariant()
                ?? throw new ArgumentNullException(nameof(email));

        var result = await _usersRepository.ExistsAsync(email);

        if (result)
        {
            _logger.LogWarning("Registration failed. User already exists: {Email}", email);
            return Result.Failure<Guid>(AuthErrors.UserAlreadyExists.Description);
        }

        var passwordHash = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        await _usersRepository.AddAsync(user);

        await _usersRepository.AddRoleAsync(user.Id, (int)RoleType.Client);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return Result.Success(user.Id);
    }

    public async Task<Result<AuthResult>> LoginAsync(string email, string password)
    {
        await _refreshTokenRepository.RemoveExpiredTokensAsync();

        email = email?.Trim().ToLowerInvariant()
                ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogWarning("Login failed. User not found: {Email}", email);
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

            UserRoles = user.Roles.Select(r => new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = (int)r
            }).ToList()
        });

        var (rawRefreshToken, hashedRefreshToken) = _refreshTokenService.GenerateToken();

        var refreshExpires = _refreshTokenService.GetRefreshTokenExpiryDate();

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
            RefreshTokenExpires = refreshExpires
        };

        return Result.Success(result);
    }

    public async Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken)
    {
        await _refreshTokenRepository.RemoveExpiredTokensAsync();

        var hashedToken = _refreshTokenService.HashToken(refreshToken);

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Refresh failed. Token not found.");
            return Result.Failure<AuthResult>(AuthErrors.InvalidRefreshToken.Description);
        }

        if (!storedToken.IsActive)
        {
            _logger.LogWarning("Refresh failed. Token revoked or expired.");
            return Result.Failure<AuthResult>(AuthErrors.InvalidRefreshToken.Description);
        }

        var user = await _usersRepository.GetByIdAsync(storedToken.UserId);

        if (user == null)
        {
            _logger.LogWarning("Refresh failed. User not found.");
            return Result.Failure<AuthResult>(AuthErrors.InvalidRefreshToken.Description);
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

        await _refreshTokenRepository.InvalidateAsync(storedToken.TokenHash);

        var refreshExpires = _refreshTokenService.GetRefreshTokenExpiryDate();

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

        return Result.Success(result);
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