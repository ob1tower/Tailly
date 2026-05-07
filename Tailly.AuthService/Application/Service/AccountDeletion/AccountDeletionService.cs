using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.Extensions.Options;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.Security.Cryptography;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Configurations.Options;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.AccountDeletion;

public class AccountDeletionService : IAccountDeletionService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAccountDeletionTokenRepository _tokenRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly FrontendOptions _frontend;

    private const int RestoreDays = 7;

    public AccountDeletionService(IUsersRepository usersRepository,
                                  IPasswordHashingService passwordHasher,
                                  IRefreshTokenRepository refreshTokenRepository,
                                  IAccountDeletionTokenRepository tokenRepository,
                                  IPublishEndpoint publishEndpoint,
                                  IOptions<FrontendOptions> options)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenRepository = tokenRepository;
        _publishEndpoint = publishEndpoint;
        _frontend = options.Value;
    }

    public async Task<Result<DateTime>> RequestDeletionAsync(Guid userId, string password, RoleType role)
    {
        if (role != RoleType.Client && role != RoleType.Specialist)
            return Result.Failure<DateTime>(AuthErrors.InvalidRole.Description);

        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure<DateTime>(AuthErrors.UserNotFound.Description); 

        var userRole = user.GetRole(role);  
        if (userRole == null)
            return Result.Failure<DateTime>(AuthErrors.InvalidRole.Description);

        if (userRole.IsEffectivelyBlocked)
            return Result.Failure<DateTime>(AuthErrors.AccountBlocked.Description);

        if (userRole.IsPendingDeletion)
            return Result.Failure<DateTime>(AuthErrors.AccountPendingDeletion.Description);

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return Result.Failure<DateTime>(AuthErrors.InvalidPassword.Description);

        var now = DateTime.UtcNow;
        var restoreUntil = now.AddDays(RestoreDays);

        var updated = await _usersRepository.PatchUserRoleSoftDeleteAsync(userId, role, now, restoreUntil);

        if (updated == 0)
            return Result.Failure<DateTime>(AuthErrors.DeletionUpdateFailed.Description);

        await _refreshTokenRepository.InvalidateAllForUserAndRoleAsync(userId, (int)role);

        var token = TokenGenerator.Generate();

        await _tokenRepository.AddAsync(new AccountDeletionToken
        {
            Token = token,
            UserId = user.Id,
            RoleId = (int)role,
            ExpiresAt = restoreUntil
        });

        var roleStr = AuthMapper.MapRole(role);

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Account scheduled for deletion",
            Body = $"""
            <h2>Account scheduled for deletion</h2>
            <p>Your {roleStr} account will be deleted in {RestoreDays} days.</p>
            <p>If this was a mistake, click the link below to restore it:</p>
            <p>
                <a href="{_frontend.BaseUrl}/account/deletion/restore?token={token}" 
                   style="color:#0066cc; font-weight:bold;">
                    Restore my account
                </a>
            </p>
            <p><small>This link will expire in {RestoreDays} days.</small></p>
        """,
            Purpose = "account-deletion"
        });

        return Result.Success(restoreUntil);
    }

    public async Task<Result> RestoreAsync(string token)
    {
        var tokenModel = await _tokenRepository.GetAsync(token);
        if (tokenModel == null || tokenModel.ExpiresAt < DateTime.UtcNow)
            return Result.Failure(AuthErrors.InvalidVerificationToken.Description);

        var user = await _usersRepository.GetByIdAsync(tokenModel.UserId);
        if (user == null)
            return Result.Failure(AuthErrors.UserNotFound.Description);

        var role = (RoleType)tokenModel.RoleId;

        var restored = await _usersRepository.PatchUserRoleSoftDeleteAsync(user.Id, role, null, null);

        if (restored == 0)
            return Result.Failure(AuthErrors.DeletionUpdateFailed.Description);

        await _tokenRepository.RemoveAsync(token);

        return Result.Success();
    }
}