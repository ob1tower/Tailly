using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.Auth.Interfaces;
using Tailly.AuthService.Application.Service.Security;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.Security.Otp;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;
using Tailly.Contracts.Messages;

namespace Tailly.AuthService.Application.Service.Auth;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IPendingRegistrationService _pendingRegistrationService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IUsersRepository usersRepository,
                                 IRefreshTokenRepository refreshTokenRepository,
                                 IPasswordHashingService passwordHasher,
                                 IJwtTokenService jwtService,
                                 IRefreshTokenService refreshTokenService,
                                 IVerificationCodeService verificationCodeService,
                                 IPendingRegistrationService pendingRegistrationService,
                                 IPublishEndpoint publishEndpoint,
                                 ILogger<AuthenticationService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _verificationCodeService = verificationCodeService;
        _pendingRegistrationService = pendingRegistrationService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<string, Error>> StartRegisterAsync(string email, string password)
    {
        email = email?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(email));

        var exists = await _usersRepository.ExistsAsync(email);

        if (exists)
        {
            _logger.LogWarning("Registration failed. User already exists: {Email}", email);
            return Result.Failure<string, Error>(AuthErrors.UserAlreadyExists);
        }

        var passwordHash = _passwordHasher.HashPassword(password);

        var registrationId = await _pendingRegistrationService.CreateAsync(email, passwordHash);

        var code = VerificationCodeGenerator.GenerateCode();

        await _verificationCodeService.SetCodeAsync(email, code, "register");

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = email,
            Subject = "Registration confirmation",
            Body = $"""
            <h2>Registration</h2>
            <p>Your verification code:</p>
            <h1>{code}</h1>
            <p>This code will expire in 15 minutes.</p>
            """,
            Purpose = "register"
        });

        _logger.LogInformation("Registration started for {Email}", email);

        return Result.Success<string, Error>(registrationId);
    }

    public async Task<Result<string, Error>> VerifyRegisterAsync(string registrationId, string code)
    {
        var data = await _pendingRegistrationService.GetAsync(registrationId);

        if (data == null)
        {
            _logger.LogWarning("Verify failed. Registration not found: {RegistrationId}", registrationId);
            return Result.Failure<string, Error>(AuthErrors.RegistrationNotFound);
        }

        var (email, _) = data.Value;

        var result = await _verificationCodeService.VerifyCodeAsync(email, code, "register");

        if (!result.Success)
        {
            _logger.LogWarning("Verify failed. Invalid code: {Email}", email);
            return Result.Failure<string, Error>(AuthErrors.InvalidVerificationCode);
        }

        var verificationToken = Guid.NewGuid().ToString();

        await _verificationCodeService.SetCodeAsync(
            verificationToken,
            "ok",
            "register-token");

        await _pendingRegistrationService.AttachTokenAsync(registrationId, verificationToken);

        _logger.LogInformation("Email verified for {Email}", email);

        return Result.Success<string, Error>(verificationToken);
    }

    public async Task<Result<AuthResult, Error>> CompleteRegisterAsync(string verificationToken, string firstName, string lastName, 
                                                                       string? middleName, string? cityName, string cityId)
    {
        var tokenValid = await _verificationCodeService.VerifyCodeAsync(
            verificationToken, "ok", "register-token");

        if (!tokenValid.Success)
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidVerificationToken);

        var data = await _pendingRegistrationService.GetByTokenAsync(verificationToken);
        if (data == null)
            return Result.Failure<AuthResult, Error>(AuthErrors.RegistrationNotFound);

        var (email, passwordHash) = data.Value;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await _usersRepository.AddAsync(user);
        await _usersRepository.AddRoleAsync(user.Id, (int)RoleType.Client);

        await _pendingRegistrationService.RemoveByTokenAsync(verificationToken);

        var (accessToken, accessExpires) = await _jwtService.CreateAccessTokenAsync(new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            UserRoles = new List<UserRoleEntity>
        {
            new UserRoleEntity { UserId = user.Id, RoleId = (int)RoleType.Client }
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
            User = AuthMapper.ToDto(user, RoleType.Client)
        };

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        await _publishEndpoint.Publish(new UserRegisteredMessage
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            CityName = cityName,
            CityId = cityId
        });

        return Result.Success<AuthResult, Error>(result);
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

        if (!user.Roles.Contains(role))
        {
            _logger.LogWarning("Login failed. Invalid role {Role} for {Email}", role, email);
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidRole);
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

    public async Task<Result<PasswordRecoveryResult>> StartPasswordRecoveryAsync(string email)
    {
        email = email?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        var flow = "default";
        if (user != null)
        {
            flow = (user.Roles.Contains(RoleType.Admin) ||
                    user.Roles.Contains(RoleType.SuperAdmin))
                   ? "admin"
                   : "default";
        }

        _logger.LogInformation("Password recovery started for {Email} with flow {Flow}", email, flow);

        return Result.Success(new PasswordRecoveryResult { Flow = flow });
    }

    public async Task<Result> SendRecoveryCodeAsync(string email)
    {
        email = email?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogInformation("Recovery code requested for non-existent email: {Email}", email);
            return Result.Success();
        }

        if (user.IsBlocked || (user.BlockedUntil != null && user.BlockedUntil > DateTime.UtcNow))
        {
            return Result.Failure(AuthErrors.AccountBlocked.Description);
        }

        var code = VerificationCodeGenerator.GenerateCode();

        await _verificationCodeService.SetCodeAsync(email, code, "password-recovery");

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = email,
            Subject = "Password Recovery Code",
            Body = $"""
            <h2>Password Recovery</h2>
            <p>Your verification code:</p>
            <h1>{code}</h1>
            <p>This code will expire in 15 minutes.</p>
            """,
            Purpose = "password-recovery"
        });

        _logger.LogInformation("Recovery code sent to {Email}", email);
        return Result.Success();
    }

    public async Task<Result> VerifyRecoveryCodeAsync(string email, string code)
    {
        email = email?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(email));

        var result = await _verificationCodeService.VerifyCodeAsync(email, code, "password-recovery", deleteAfterVerify: false);

        if (!result.Success)
        {
            _logger.LogWarning("Recovery code verification failed for {Email}", email);
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        _logger.LogInformation("Recovery code verified successfully for {Email}", email);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string email, string code, string newPassword)
    {
        email = email?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        if (user.IsBlocked || (user.BlockedUntil != null && user.BlockedUntil > DateTime.UtcNow))
        {
            return Result.Failure(AuthErrors.AccountBlocked.Description);
        }

        if (_passwordHasher.VerifyPassword(newPassword, user.PasswordHash))
        {
            return Result.Failure(AuthErrors.SamePassword.Description);
        }

        var codeResult = await _verificationCodeService.VerifyCodeAsync(email, code, "password-recovery");
        if (!codeResult.Success)
        {
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        var newHash = _passwordHasher.HashPassword(newPassword);
        user.PasswordHash = newHash;

        await _usersRepository.UpdateAsync(user);
        await _refreshTokenRepository.InvalidateAllAsync(user.Id);

        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("ChangePassword failed. User not found: {UserId}", userId);
            return Result.Failure(AuthErrors.InvalidCredentials.Description);
        }

        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
        {
            _logger.LogWarning("ChangePassword failed. Invalid current password for user: {UserId}", userId);
            return Result.Failure(AuthErrors.InvalidPassword.Description);
        }

        if (_passwordHasher.VerifyPassword(newPassword, user.PasswordHash))
        {
            _logger.LogWarning("ChangePassword failed. New password same as old for user: {UserId}", userId);
            return Result.Failure(AuthErrors.SamePassword.Description);
        }

        var newHash = _passwordHasher.HashPassword(newPassword);
        user.PasswordHash = newHash;
        await _usersRepository.UpdateAsync(user);

        await _refreshTokenRepository.InvalidateAllAsync(userId);

        _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result<EmailChangeResult, Error>> RequestEmailChangeAsync(Guid userId, string newEmail)
    {
        newEmail = newEmail?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(newEmail));

        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("RequestEmailChange failed. User not found: {UserId}", userId);
            return Result.Failure<EmailChangeResult, Error>(AuthErrors.InvalidCredentials);
        }

        if (user.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("RequestEmailChange failed. Same email for user: {UserId}", userId);
            return Result.Failure<EmailChangeResult, Error>(AuthErrors.SameEmail);
        }

        if (await _usersRepository.ExistsAsync(newEmail))
        {
            _logger.LogWarning("RequestEmailChange failed. Email already exists: {NewEmail}", newEmail);
            return Result.Failure<EmailChangeResult, Error>(AuthErrors.UserAlreadyExists);
        }

        var requestId = Guid.NewGuid().ToString();
        var code = VerificationCodeGenerator.GenerateCode();

        await _verificationCodeService.SetCodeAsync(newEmail, code, "change-email");

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = newEmail,
            Subject = "Email Change Confirmation",
            Body = $"""
            <h2>Email Change Request</h2>
            <p>Your confirmation code:</p>
            <h1>{code}</h1>
            <p>The code is valid for 15 minutes.</p>
            """,
            Purpose = "change-email"
        });

        var maskedOldEmail = EmailHelper.Mask(user.Email);

        _logger.LogInformation("Email change requested for user {UserId} → {NewEmail} (RequestId: {RequestId})",
            userId, newEmail, requestId);

        var emailChangeResult = new EmailChangeResult
        {
            RequestId = requestId,
            MaskedOldEmail = maskedOldEmail
        };

        return Result.Success<EmailChangeResult, Error>(emailChangeResult);
    }

    public async Task<Result> ConfirmEmailChangeAsync(Guid userId, string requestId, string newEmail, string code)
    {
        newEmail = newEmail?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(newEmail));

        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("ConfirmEmailChange failed. User not found: {UserId}", userId);
            return Result.Failure(AuthErrors.InvalidCredentials.Description);
        }

        var verifyResult = await _verificationCodeService.VerifyCodeAsync(newEmail, code, "change-email");
        if (!verifyResult.Success)
        {
            _logger.LogWarning("ConfirmEmailChange failed. Invalid code for new email: {NewEmail}", newEmail);
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        user.Email = newEmail;
        user.EmailConfirmed = true;
        await _usersRepository.UpdateAsync(user);

        await _refreshTokenRepository.InvalidateAllAsync(userId);

        _logger.LogInformation("Email changed successfully for user {UserId} to {NewEmail}", userId, newEmail);
        return Result.Success();
    }
}