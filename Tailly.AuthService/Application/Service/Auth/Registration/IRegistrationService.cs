using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.Auth.Registration;

public interface IRegistrationService
{
    Task<Result<AuthResult, Error>> CompleteRegisterAsync(string verificationToken, string firstName, string lastName, string? middleName, string? cityName, string cityId);
    Task<Result<string, Error>> StartRegisterAsync(string email, string password);
    Task<Result<string, Error>> VerifyRegisterAsync(string registrationId, string code);
}