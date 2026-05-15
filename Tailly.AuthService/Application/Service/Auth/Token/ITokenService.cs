using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.Auth.Token;

public interface ITokenService
{
    Task<Result> LogoutAsync(string refreshToken);
    Task<Result<AuthResult, Error>> RefreshTokenAsync(string refreshToken);
}