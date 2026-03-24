using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.AuthService.Entities;
using Tailly.AuthService.Enums;
using Tailly.AuthService.Errors;
using Tailly.AuthService.Models;
using Tailly.AuthService.RabbitMq.Messages;
using Tailly.AuthService.Repositories.Interfaces;
using Tailly.AuthService.Service.Security;
using Tailly.AuthService.Service.Security.Otp;
using Tailly.AuthService.Service.Tokens;

namespace Tailly.AuthService.Service.Auth;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IUsersRepository usersRepository,
                       IRefreshTokenRepository refreshTokenRepository,
                       IPasswordHashingService passwordHasher,
                       IJwtTokenService jwtService,
                       IRefreshTokenService refreshTokenService,
                       IVerificationCodeService verificationCodeService,
                       IPublishEndpoint publishEndpoint,
                       ILogger<AuthenticationService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _verificationCodeService = verificationCodeService;
        _publishEndpoint = publishEndpoint;
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
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = false
        };

        await _usersRepository.AddAsync(user);

        await _usersRepository.AddRoleAsync(user.Id, (int)RoleType.Client);

        var code = VerificationCodeGenerator.GenerateCode();
        await _verificationCodeService.SetCodeAsync(email, code);

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Email confirmation",
            Body = $"""
            <h2>Email confirmation</h2>
            <p>Your verification code:</p>
            <h1>{code}</h1>
            <p>This code will expire in 15 minutes.</p>
            """,
            Purpose = "confirmation"
        });

        _logger.LogInformation("Confirmation email message published to RabbitMQ for {Email}", email);

        return Result.Success(user.Id);
    }

    public async Task<Result<AuthResult>> LoginAsync(string email, string password)
    {
        email = email?.Trim().ToLowerInvariant()
                ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogWarning("Login failed. User not found: {Email}", email);
            return Result.Failure<AuthResult>(AuthErrors.InvalidCredentials.Description);
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login failed. Email not confirmed: {Email}", email);
            return Result.Failure<AuthResult>(AuthErrors.EmailNotConfirmed.Description);
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
        var hashedToken = _refreshTokenService.HashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Refresh failed. Token not found.");
            return Result.Failure<AuthResult>(AuthErrors.InvalidRefreshToken.Description);
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

            return Result.Failure<AuthResult>(AuthErrors.InvalidRefreshToken.Description);
        }

        var user = await _usersRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
        {
            _logger.LogWarning("Refresh failed. User not found for token.");
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

    public async Task<Result> ConfirmEmailAsync(string email, string code)
    {
        email = email?.Trim().ToLowerInvariant()
                ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogWarning("ConfirmEmail failed. User not found: {Email}", email);
            return Result.Failure(AuthErrors.InvalidCredentials.Description);
        }

        if (user.EmailConfirmed)
        {
            _logger.LogInformation("Email already confirmed: {Email}", email);
            return Result.Success();
        }

        var result = await _verificationCodeService.VerifyCodeAsync(email, code);

        if (!result.Success)
        {
            _logger.LogWarning("ConfirmEmail failed. Invalid or expired code: {Email}", email);
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        user.EmailConfirmed = true;

        await _usersRepository.UpdateAsync(user);

        _logger.LogInformation("Email confirmed successfully: {UserId}", user.Id);

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        email = email?.Trim().ToLowerInvariant()
                ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
            return Result.Success();
        }

        var code = VerificationCodeGenerator.GenerateCode();
        await _verificationCodeService.SetCodeAsync(email, code);

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = email,
            Subject = "Password reset",
            Body = $"""
            <h2>Password reset</h2>
            <p>Your reset code:</p>
            <h1>{code}</h1>
            <p>This code will expire in 15 minutes.</p>
            """,
            Purpose = "password-reset"
        });

        _logger.LogInformation("Password reset message published to RabbitMQ for {Email}", email);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string email, string code, string newPassword)
    {
        email = email?.Trim().ToLowerInvariant()
                ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogInformation("Reset password attempted for non-existent email: {Email}", email);
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        if (_passwordHasher.VerifyPassword(newPassword, user.PasswordHash))
        {
            _logger.LogWarning("ResetPassword failed. Same password: {Email}", email);
            return Result.Failure(AuthErrors.SamePassword.Description);
        }

        var result = await _verificationCodeService.VerifyCodeAsync(email, code);

        if (!result.Success)
        {
            _logger.LogWarning("ResetPassword failed. Invalid or expired code: {Email}", email);
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        var newHash = _passwordHasher.HashPassword(newPassword);

        user.PasswordHash = newHash;

        await _usersRepository.UpdateAsync(user);

        _logger.LogInformation("Password reset successfully: {UserId}", user.Id);

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

        var isValid = _passwordHasher.VerifyPassword(currentPassword, user.PasswordHash);

        if (!isValid)
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

        _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

        return Result.Success();
    }

    public async Task<Result> RequestEmailChangeAsync(Guid userId, string newEmail)
    {
        newEmail = newEmail?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(newEmail));

        var user = await _usersRepository.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("ChangeEmail request failed. User not found: {UserId}", userId);
            return Result.Failure(AuthErrors.InvalidCredentials.Description);
        }

        if (user.Email == newEmail)
        {
            _logger.LogWarning("ChangeEmail request failed. Same email: {Email}", newEmail);
            return Result.Failure(AuthErrors.SameEmail.Description);
        }

        var exists = await _usersRepository.ExistsAsync(newEmail);

        if (exists)
        {
            _logger.LogWarning("ChangeEmail request failed. Email already exists: {Email}", newEmail);
            return Result.Failure(AuthErrors.UserAlreadyExists.Description);
        }

        var code = VerificationCodeGenerator.GenerateCode();

        await _verificationCodeService.SetCodeAsync(newEmail, code, "change-email");

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = newEmail,
            Subject = "Confirm email change",
            Body = $"""
            <h2>Email change</h2>
            <p>Your verification code:</p>
            <h1>{code}</h1>
            <p>This code will expire in 15 minutes.</p>
            """,
            Purpose = "change-email"
        });

        _logger.LogInformation("Email change message published to RabbitMQ for {Email}", newEmail);

        return Result.Success();
    }

    public async Task<Result> ConfirmEmailChangeAsync(Guid userId, string newEmail, string code)
    {
        newEmail = newEmail?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(newEmail));

        var user = await _usersRepository.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("ConfirmEmailChange failed. User not found: {UserId}", userId);
            return Result.Failure(AuthErrors.InvalidCredentials.Description);
        }

        var result = await _verificationCodeService.VerifyCodeAsync(newEmail, code, "change-email");

        if (!result.Success)
        {
            _logger.LogWarning("ConfirmEmailChange failed. Invalid code for {Email}", newEmail);
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        user.Email = newEmail;
        user.EmailConfirmed = true;

        await _usersRepository.UpdateAsync(user);

        await _refreshTokenRepository.InvalidateAllAsync(userId);

        _logger.LogInformation("Email changed successfully for user: {UserId}", userId);

        return Result.Success();
    }
}