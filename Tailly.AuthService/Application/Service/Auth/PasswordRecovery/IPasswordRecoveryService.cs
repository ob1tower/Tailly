using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.Auth.PasswordRecovery;

public interface IPasswordRecoveryService
{
    Task<Result> ResetPasswordAsync(string email, string code, string newPassword);
    Task<Result> SendRecoveryCodeAsync(string email);
    Task<Result<PasswordRecoveryResult>> StartPasswordRecoveryAsync(string email);
    Task<Result> VerifyRecoveryCodeAsync(string email, string code);
}