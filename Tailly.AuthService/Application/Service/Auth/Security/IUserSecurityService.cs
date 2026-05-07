using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.Auth.Security
{
    public interface IUserSecurityService
    {
        Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<Result> ConfirmEmailChangeAsync(Guid userId, string requestId, string newEmail, string code);
        Task<Result<EmailChangeResult, Error>> RequestEmailChangeAsync(Guid userId, string newEmail);
        Task<Result<EmailChangeResult, Error>> EmailChangeAsync(Guid userId, string newEmail, string password);
        Task<Result> ConfirmEmailAdminChangeAsync(Guid userId, string code);
        Task<Result> CancelEmailChangeAsync(Guid userId);
    }
}