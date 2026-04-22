using CSharpFunctionalExtensions;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.Auth.Login;

public class LoginService : ILoginService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LoginService> _logger;

    public LoginService(IUsersRepository usersRepository,
                        IRefreshTokenRepository refreshTokenRepository,
                        IPasswordHashingService passwordHasher,
                        IJwtTokenService jwtService,
                        IRefreshTokenService refreshTokenService,
                        ILogger<LoginService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResult, Error>> LoginAsync(string email, string password, RoleType role)
    {
        email = email?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogWarning("Login failed. User not found: {Email}", email);
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidCredentials);
        }

        if (user.SoftDeletedAt != null && user.RestoreUntil > DateTime.UtcNow)
        {
            _logger.LogWarning("Login failed. Account pending deletion: {Email}", email);
            return Result.Failure<AuthResult, Error>(AuthErrors.AccountPendingDeletion);
        }

        if (user.IsBlocked || (user.BlockedUntil != null && user.BlockedUntil > DateTime.UtcNow))
        {
            _logger.LogWarning("Login failed. Account blocked: {Email}", email);
            return Result.Failure<AuthResult, Error>(AuthErrors.AccountBlocked);
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login failed. Email not confirmed: {Email}", email);
            return Result.Failure<AuthResult, Error>(AuthErrors.EmailNotConfirmed);
        }

        var validPassword = _passwordHasher.VerifyPassword(password, user.PasswordHash);

        if (!validPassword)
        {
            _logger.LogWarning("Login failed. Invalid password for {Email}", email);
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidCredentials);
        }

        if (user.Roles.Contains(RoleType.Admin) || user.Roles.Contains(RoleType.SuperAdmin))
        {
            if (user.Roles.Count > 1)
            {
                _logger.LogError("Invalid role configuration for admin user {UserId}", user.Id);
                return Result.Failure<AuthResult, Error>(AuthErrors.AccessDenied);
            }

            var actualRole = user.Roles.First();

            if (role != actualRole)
            {
                _logger.LogWarning("Admin login with wrong role. Requested: {Requested}, Actual: {Actual}", role, actualRole);
                return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRole);
            }
        }
        else
        {
            if (!user.Roles.Contains(role))
            {
                _logger.LogWarning("Login failed. Invalid role {Role} for {Email}", role, email);
                return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRole);
            }
        }

        var (accessToken, accessExpires) = await _jwtService.CreateAccessTokenAsync(new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            UserRoles = new List<UserRoleEntity>
        {
            new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = (int)role
            }
        }
        });

        var (rawRefreshToken, hashedRefreshToken) = _refreshTokenService.GenerateToken();
        var refreshExpires = _refreshTokenService.GetRefreshTokenExpiryDate();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = hashedRefreshToken,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = refreshExpires
        });

        var result = new AuthResult
        {
            AccessToken = accessToken,
            AccessTokenExpires = accessExpires,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpires = refreshExpires,
            User = AuthMapper.ToDto(user, role)
        };

        _logger.LogInformation("User logged in successfully: {UserId} with role {Role}", user.Id, role);

        return Result.Success<AuthResult, Error>(result);
    }
}
