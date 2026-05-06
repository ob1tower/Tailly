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
    private readonly IAdminProfileRepository _adminProfileRepository;
    private readonly ILogger<LoginService> _logger;

    public LoginService(IUsersRepository usersRepository,
                        IRefreshTokenRepository refreshTokenRepository,
                        IPasswordHashingService passwordHasher,
                        IJwtTokenService jwtService,
                        IRefreshTokenService refreshTokenService,
                        IAdminProfileRepository adminProfileRepository,
                        ILogger<LoginService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _adminProfileRepository = adminProfileRepository;
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

        if (role == RoleType.Admin)
        {
            var adminRoles = user.UserRoles
                .Where(r => (r.Role == RoleType.Admin)
                         && r.SoftDeletedAt == null)
                .ToList();

            if (adminRoles.Count == 0)
            {
                _logger.LogWarning("Login failed. No admin role found for {Email}", email);
                return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRole);
            }

            if (adminRoles.Count > 1)
            {
                _logger.LogError("Invalid role configuration: User {UserId} has multiple admin roles", user.Id);
                return Result.Failure<AuthResult, Error>(AuthErrors.AccessDenied);
            }

            var userRole = adminRoles.First();

            if (userRole.Role != role)
            {
                _logger.LogWarning("Admin login with wrong role. Requested: {Requested}, Actual: {Actual}",
                    role, userRole.Role);
                return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRole);
            }

            if (userRole.IsPendingDeletion || userRole.IsEffectivelyBlocked)
            {
                _logger.LogWarning("Login failed. Admin role is blocked or pending deletion: {Email}", email);
                return Result.Failure<AuthResult, Error>(AuthErrors.AccountBlocked);
            }
        }
        else
        {
            var userRole = user.GetRole(role);
            if (userRole == null)
            {
                _logger.LogWarning("Login failed. Role {Role} not found for {Email}", role, email);
                return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRole);
            }

            if (userRole.IsPendingDeletion)
            {
                _logger.LogWarning("Login failed. Role is pending deletion: {Email} role {Role}", email, role);
                return Result.Failure<AuthResult, Error>(AuthErrors.AccountPendingDeletion);
            }

            if (userRole.IsEffectivelyBlocked)
            {
                _logger.LogWarning("Login failed. Role is blocked: {Email} role {Role}", email, role);
                return Result.Failure<AuthResult, Error>(AuthErrors.AccountBlocked);
            }
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login failed. Email not confirmed: {Email}", email);
            return Result.Failure<AuthResult, Error>(AuthErrors.EmailNotConfirmed);
        }

        if (role == RoleType.Admin)
        {
            var profile = await _adminProfileRepository.GetByUserIdAsync(user.Id);
            if (profile != null && profile.PasswordAttemptsLockUntil.HasValue &&
                profile.PasswordAttemptsLockUntil > DateTime.UtcNow)
            {
                _logger.LogWarning("Login failed. Admin account is temporarily locked due to too many failed attempts. UserId={UserId}", user.Id);
                return Result.Failure<AuthResult, Error>(AuthErrors.AccountTemporarilyLocked);
            }
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed. Invalid password for {Email}", email);

            if (role == RoleType.Admin)
            {
                var profile = await _adminProfileRepository.GetByUserIdAsync(user.Id);
                if (profile != null)
                {
                    profile.FailedPasswordAttempts++;

                    if (profile.FailedPasswordAttempts >= 5)
                    {
                        profile.PasswordAttemptsLockUntil = DateTime.UtcNow.AddMinutes(15);
                        _logger.LogWarning("Admin account locked due to too many failed attempts. UserId={UserId}", user.Id);
                    }

                    await _adminProfileRepository.UpdateAsync(profile);
                }
            }

            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidCredentials);
        }

        var userRoleForUpdate = user.GetRole(role);
        if (userRoleForUpdate != null &&
            userRoleForUpdate.IsBlocked &&
            !userRoleForUpdate.IsPermanentBlock &&
            userRoleForUpdate.BlockedUntil <= DateTime.UtcNow)
        {
            userRoleForUpdate.IsBlocked = false;
            userRoleForUpdate.BlockedUntil = null;
            userRoleForUpdate.BlockReason = null;
            await _usersRepository.UpdateAsync(user);
        }

        if (role == RoleType.Admin)
        {
            var profile = await _adminProfileRepository.GetByUserIdAsync(user.Id);
            if (profile != null)
            {
                profile.LastLoginAt = DateTime.UtcNow;
                await _adminProfileRepository.UpdateAsync(profile);
            }
        }

        var userEntity = new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            SpecialistId = user.SpecialistId,

            UserRoles = user.UserRoles.Select(r => new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = (int)r.Role
            }).ToList()
        };

        var (accessToken, accessExpires) = await _jwtService.CreateAccessTokenAsync(userEntity);

        var (rawRefreshToken, hashedRefreshToken) = _refreshTokenService.GenerateToken();
        var refreshExpires = _refreshTokenService.GetRefreshTokenExpiryDate();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = hashedRefreshToken,
            UserId = user.Id,
            RoleId = (int)role,
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