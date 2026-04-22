using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Security.Cryptography;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Models;
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

    private const int RestoreDays = 7;

    public AccountDeletionService(IUsersRepository usersRepository,
                                  IPasswordHashingService passwordHasher,
                                  IRefreshTokenRepository refreshTokenRepository,
                                  IAccountDeletionTokenRepository tokenRepository,
                                  IPublishEndpoint publishEndpoint)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenRepository = tokenRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<DateTime>> RequestDeletionAsync(Guid userId, string password)
    {
        var user = await _usersRepository.GetByIdAsync(userId);

        if (user == null)
            return Result.Failure<DateTime>(AuthErrors.InvalidCredentials.Description);

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return Result.Failure<DateTime>(AuthErrors.InvalidPassword.Description);

        var now = DateTime.UtcNow;
        var restoreUntil = now.AddDays(RestoreDays);

        user.SoftDeletedAt = now;
        user.RestoreUntil = restoreUntil;

        await _usersRepository.UpdateAsync(user);

        await _refreshTokenRepository.InvalidateAllAsync(user.Id);

        var token = TokenGenerator.Generate();

        await _tokenRepository.AddAsync(new AccountDeletionToken
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = restoreUntil
        });

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Account deletion",
            Body = $"Restore your account: https://frontend/restore?token={token}"
        });

        return Result.Success((restoreUntil));
    }

    public async Task<Result<(string email, string role, DateTime restoreUntil)>> GetRestorePreviewAsync(string token)
    {
        var tokenModel = await _tokenRepository.GetAsync(token);

        if (tokenModel == null || tokenModel.ExpiresAt < DateTime.UtcNow)
            return Result.Failure<(string, string, DateTime)>(AuthErrors.InvalidVerificationToken.Description);

        var user = await _usersRepository.GetByIdAsync(tokenModel.UserId);

        if (user == null)
            return Result.Failure<(string, string, DateTime)>(AuthErrors.InvalidCredentials.Description);

        var role = user.Roles.First().ToString().ToLower();

        return Result.Success((user.Email, role, tokenModel.ExpiresAt));
    }

    public async Task<Result> RestoreAsync(string token)
    {
        var tokenModel = await _tokenRepository.GetAsync(token);

        if (tokenModel == null || tokenModel.ExpiresAt < DateTime.UtcNow)
            return Result.Failure(AuthErrors.InvalidVerificationToken.Description);

        var user = await _usersRepository.GetByIdAsync(tokenModel.UserId);

        if (user == null)
            return Result.Failure(AuthErrors.InvalidCredentials.Description);

        user.SoftDeletedAt = null;
        user.RestoreUntil = null;

        await _usersRepository.UpdateAsync(user);

        await _tokenRepository.RemoveAsync(token);

        return Result.Success();
    }
}