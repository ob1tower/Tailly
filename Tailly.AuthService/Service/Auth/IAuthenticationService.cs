using CSharpFunctionalExtensions;
using Tailly.AuthService.Models;

namespace Tailly.AuthService.Service.Auth
{
    public interface IAuthenticationService
    {
        Task<Result<AuthResult>> LoginAsync(string email, string password);
        Task<Result> LogoutAsync(string refreshToken);
        Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken);
        Task<Result<Guid>> RegisterAsync(string email, string password);
    }
}