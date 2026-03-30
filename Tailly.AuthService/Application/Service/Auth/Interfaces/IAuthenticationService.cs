using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.Auth.Interfaces;

public interface IAuthenticationService
{
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<Result<AuthResult, Error>> CompleteRegisterAsync(string registrationId, string verificationToken);
    Task<Result> ConfirmEmailChangeAsync(Guid userId, string requestId, string newEmail, string code);
    Task<Result<AuthResult, Error>> LoginAsync(string email, string password, RoleType role);
    Task<Result> LogoutAsync(string refreshToken);
    Task<Result<AuthResult, Error>> RefreshTokenAsync(string refreshToken);
    Task<Result<EmailChangeResult, Error>> RequestEmailChangeAsync(Guid userId, string newEmail);
    Task<Result> ResetPasswordAsync(string email, string code, string newPassword);
    Task<Result> SendRecoveryCodeAsync(string email);
    Task<Result<PasswordRecoveryResult>> StartPasswordRecoveryAsync(string email);
    Task<Result<string, Error>> StartRegisterAsync(string email, string password);
    Task<Result> VerifyRecoveryCodeAsync(string email, string code);
    Task<Result<string, Error>> VerifyRegisterAsync(string registrationId, string code);
}