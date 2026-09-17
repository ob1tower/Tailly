using CSharpFunctionalExtensions;
using MassTransit;
using StackExchange.Redis;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Security;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.Security.Otp;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.Auth.Security;

public class UserSecurityService : IUserSecurityService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDatabase _redis;
    private readonly ILogger<UserSecurityService> _logger;

    public UserSecurityService(IUsersRepository usersRepository,
                               IPasswordHashingService passwordHasher,
                               IVerificationCodeService verificationCodeService,
                               IRefreshTokenRepository refreshTokenRepository,
                               IPublishEndpoint publishEndpoint,
                               IConnectionMultiplexer redis,
                               ILogger<UserSecurityService> logger)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _verificationCodeService = verificationCodeService;
        _refreshTokenRepository = refreshTokenRepository;
        _publishEndpoint = publishEndpoint;
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("ChangePassword failed. User not found: {UserId}", userId);
            return Result.Failure(AuthErrors.InvalidCredentials.Description);
        }

        var activeRoles = user.UserRoles.Where(r => r.SoftDeletedAt == null).ToList();
        if (activeRoles.Count == 0)
        {
            _logger.LogWarning("ChangePassword failed. No active roles for user: {UserId}", userId);
            return Result.Failure(AuthErrors.AccountPendingDeletion.Description);
        }

        if (activeRoles.All(r => r.IsEffectivelyBlocked))
        {
            _logger.LogWarning("ChangePassword failed. All roles are blocked for user: {UserId}", userId);
            return Result.Failure(AuthErrors.AccountBlocked.Description);
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

        var activeRoles = user.UserRoles.Where(r => r.SoftDeletedAt == null).ToList();
        if (activeRoles.Count == 0)
        {
            _logger.LogWarning("RequestEmailChange failed. No active roles for user: {UserId}", userId);
            return Result.Failure<EmailChangeResult, Error>(AuthErrors.AccountPendingDeletion);
        }

        if (activeRoles.All(r => r.IsEffectivelyBlocked))
        {
            _logger.LogWarning("RequestEmailChange failed. All roles are blocked for user: {UserId}", userId);
            return Result.Failure<EmailChangeResult, Error>(AuthErrors.AccountBlocked);
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

        var code = VerificationCodeGenerator.GenerateCode();

        var newEmailKey = $"email-change:new:{userId}";
        await _redis.StringSetAsync(newEmailKey, newEmail, TimeSpan.FromMinutes(15));

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Email Change Confirmation",
            Body = $"""
            <h2>Email Change Request</h2>
            <p>Someone requested to change your email to: <b>{newEmail}</b></p>
            <p>Your confirmation code:</p>
            <h1>{code}</h1>
            <p>The code is valid for 15 minutes.</p>
            """,
            Purpose = "change-email"
        });

        await _verificationCodeService.SetCodeAsync(user.Email, code, "change-email");

        var maskedOldEmail = EmailHelper.Mask(user.Email);

        _logger.LogInformation("Email change requested for user {UserId} → {NewEmail}", userId, newEmail);

        return Result.Success<EmailChangeResult, Error>(new EmailChangeResult
        {
            RequestId = Guid.NewGuid().ToString(),
            MaskedOldEmail = maskedOldEmail
        });
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

        var activeRoles = user.UserRoles.Where(r => r.SoftDeletedAt == null).ToList();
        if (activeRoles.Count == 0)
        {
            _logger.LogWarning("ConfirmEmailChange failed. No active roles for user: {UserId}", userId);
            return Result.Failure(AuthErrors.AccountPendingDeletion.Description);
        }

        if (activeRoles.All(r => r.IsEffectivelyBlocked))
        {
            _logger.LogWarning("ConfirmEmailChange failed. All roles are blocked for user: {UserId}", userId);
            return Result.Failure(AuthErrors.AccountBlocked.Description);
        }

        var verifyResult = await _verificationCodeService.VerifyCodeAsync(user.Email, code, "change-email");
        if (!verifyResult.Success)
        {
            _logger.LogWarning("ConfirmEmailChange failed. Invalid code for user: {UserId}", userId);
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        var newEmailKey = $"email-change:new:{userId}";
        var newEmailValue = await _redis.StringGetAsync(newEmailKey);

        if (newEmailValue.IsNullOrEmpty || !newEmailValue.ToString()!.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("ConfirmEmailChange failed. New email mismatch or not found in Redis. UserId: {UserId}", userId);
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);
        }

        user.Email = newEmail;
        user.EmailConfirmed = true;

        await _usersRepository.UpdateAsync(user);
        await _refreshTokenRepository.InvalidateAllAsync(userId);
        await _redis.KeyDeleteAsync(newEmailKey);

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = newEmail,
            Subject = "Email successfully changed",
            Body = $"""
            <h2>Your email has been successfully changed</h2>
            <p>New email: {newEmail}</p>
            """,
            Purpose = "email-changed"
        });

        _logger.LogInformation("Email changed successfully for user {UserId} to {NewEmail}", userId, newEmail);
        return Result.Success();
    }

    public async Task<Result<EmailChangeResult, Error>> EmailChangeAsync(Guid userId, string newEmail, string password)
    {
        newEmail = newEmail?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(newEmail));

        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure<EmailChangeResult, Error>(AdminErrors.UserNotFound);

        var isSuperAdmin = user.UserRoles.Any(r =>
            r.Role == RoleType.SuperAdmin && r.SoftDeletedAt == null);

        if (!isSuperAdmin)
            return Result.Failure<EmailChangeResult, Error>(AdminErrors.OnlySuperAdminCanChangeEmail);

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return Result.Failure<EmailChangeResult, Error>(AuthErrors.InvalidPassword);

        if (user.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<EmailChangeResult, Error>(AdminErrors.SameEmail);

        if (await _usersRepository.ExistsAsync(newEmail))
            return Result.Failure<EmailChangeResult, Error>(AdminErrors.EmailAlreadyExists);

        var code = VerificationCodeGenerator.GenerateCode();

        var newEmailKey = $"email-change:new:{userId}";
        await _redis.StringSetAsync(newEmailKey, newEmail, TimeSpan.FromMinutes(15));

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Email confirmation of the change",
            Body = $"""
            <h2>Changing the email address of the main administrator</h2>
            <p>Confirmation code:</p>
            <h1>{code}</h1>
            <p>The code is valid for 15 minutes.</p>
            """,
            Purpose = "admin-change-email"
        });

        await _verificationCodeService.SetCodeAsync(user.Email, code, "admin-change-email");

        var maskedOldEmail = EmailHelper.Mask(user.Email);

        _logger.LogInformation("SuperAdmin requested email change. UserId={UserId}, NewEmail={NewEmail}", userId, newEmail);

        return Result.Success<EmailChangeResult, Error>(new EmailChangeResult
        {
            RequestId = Guid.NewGuid().ToString(),
            MaskedOldEmail = maskedOldEmail
        });
    }

    public async Task<Result> ConfirmEmailAdminChangeAsync(Guid userId, string code)
    {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure(AdminErrors.UserNotFound.Description);

        var verifyResult = await _verificationCodeService.VerifyCodeAsync(
            user.Email, code, "admin-change-email");

        if (!verifyResult.Success)
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);

        var newEmailKey = $"email-change:new:{userId}";
        var newEmailValue = await _redis.StringGetAsync(newEmailKey);

        if (newEmailValue.IsNullOrEmpty)
            return Result.Failure(AdminErrors.NewEmailNotFound.Description);

        var newEmail = newEmailValue.ToString()!;

        user.Email = newEmail;
        user.EmailConfirmed = true;

        await _usersRepository.UpdateAsync(user);
        await _refreshTokenRepository.InvalidateAllAsync(userId);
        await _redis.KeyDeleteAsync(newEmailKey);

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = newEmail,
            Subject = "Email successfully changed",
            Body = $"""
            <h2>Your email has been successfully changed</h2>
            <p>New email: {newEmail}</p>
            """,
            Purpose = "email-changed"
        });

        _logger.LogInformation("SuperAdmin email changed successfully. UserId={UserId}, NewEmail={NewEmail}", userId, newEmail);
        return Result.Success();
    }

    public async Task<Result> CancelEmailChangeAsync(Guid userId)
    {
        await _verificationCodeService.RemoveCodeAsync(userId.ToString(), "admin-change-email");
        _logger.LogInformation("Email change cancelled for user {UserId}", userId);
        return Result.Success();
    }
}