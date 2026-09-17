using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Helpers;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.Security.Otp;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.Auth.PasswordRecovery;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAdminPasswordRecoveryRepository _passwordRecoveryRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PasswordRecoveryService> _logger;

    public PasswordRecoveryService(IUsersRepository usersRepository,
                                   IPasswordHashingService passwordHasher,
                                   IVerificationCodeService verificationCodeService,
                                   IRefreshTokenRepository refreshTokenRepository,
                                   IAdminPasswordRecoveryRepository passwordRecoveryRepository,
                                   IPublishEndpoint publishEndpoint,
                                   ILogger<PasswordRecoveryService> logger)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _verificationCodeService = verificationCodeService;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordRecoveryRepository = passwordRecoveryRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<PasswordRecoveryResult>> StartPasswordRecoveryAsync(string email)
    {
        email = email?.Trim().ToLowerInvariant() ??
                    throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);

        var flow = "default";

        if (user != null)
        {
            var isAdmin = user.UserRoles.Any(x =>
                x.SoftDeletedAt == null &&
                x.Role == RoleType.Admin);

            if (isAdmin)
            {
                flow = "admin";

                if (UserRules.IsPasswordRecoveryBlockedForAllRoles(user))
                    return Result.Failure<PasswordRecoveryResult>(AuthErrors.AccountBlocked.Description);

                var hasPendingRequest = await _passwordRecoveryRepository
                    .GetAllAsync()
                    .ContinueWith(t => t.Result.Any(r =>
                        r.Email == email &&
                        r.Status == AdminPasswordRecoveryStatus.Pending));

                if (hasPendingRequest)
                    return Result.Failure<PasswordRecoveryResult>(AdminErrors.RequestAlreadyPending.Description);

                var recovery = new AdminPasswordRecovery
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    RequestedAt = DateTimeHelper.NormalizeToUtc(DateTime.UtcNow),
                    Status = AdminPasswordRecoveryStatus.Pending
                };

                await _passwordRecoveryRepository.AddAsync(recovery);

                await _publishEndpoint.Publish(new SendEmailMessage
                {
                    To = email,
                    Subject = "Password Recovery Request",
                    Body = $"""
                    <h2>Password Recovery Request</h2>
                    <p>You have requested a password reset for your account <strong>{email}</strong>.</p>
                    <p>Your application has been accepted and submitted to the administrator.</p>
                    """,
                    Purpose = "admin-password-recovery-request"
                });

                _logger.LogInformation("Admin password recovery requested. Email={Email}", email);
            }
        }

        return Result.Success(new PasswordRecoveryResult { Flow = flow });
    }

    public async Task<Result> SendRecoveryCodeAsync(string email)
    {
        email = email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);
        if (user == null)
        {
            _logger.LogInformation("Recovery code requested for non-existent email: {Email}", email);
            return Result.Success();
        }

        if (UserRules.IsPasswordRecoveryBlockedForAllRoles(user))
            return Result.Failure(AuthErrors.AccountBlocked.Description);

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
        email = email?.Trim().ToLowerInvariant() ?? 
            throw new ArgumentNullException(nameof(email));

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
        email = email?.Trim().ToLowerInvariant() ?? 
            throw new ArgumentNullException(nameof(email));

        var user = await _usersRepository.GetByEmailAsync(email);
        if (user == null)
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);

        if (UserRules.IsPasswordRecoveryBlockedForAllRoles(user))
            return Result.Failure(AuthErrors.AccountBlocked.Description);

        if (_passwordHasher.VerifyPassword(newPassword, user.PasswordHash))
            return Result.Failure(AuthErrors.SamePassword.Description);

        var codeResult = await _verificationCodeService.VerifyCodeAsync(email, code, "password-recovery");
        if (!codeResult.Success)
            return Result.Failure(AuthErrors.InvalidVerificationCode.Description);

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);

        await _usersRepository.UpdateAsync(user);
        await _refreshTokenRepository.InvalidateAllAsync(user.Id);

        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
        return Result.Success();
    }
}