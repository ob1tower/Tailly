using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.Auth.Login
{
    public interface ILoginService
    {
        Task<Result<AuthResult, Error>> LoginAsync(string email, string password, RoleType role);
    }
}