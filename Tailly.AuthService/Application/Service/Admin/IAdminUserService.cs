using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.Admin
{
    public interface IAdminUserService
    {
        Task<Result<AdminUserResult, Error>> GetAllAsync(string? search, string? role, int page, int pageSize);
        Task<Result<AdminUser, Error>> GetByIdAsync(Guid id);
        Task<Result<bool, Error>> RestoreFromDeletionAsync(Guid id);
        Task<Result<bool, Error>> UpdateBlockStatusAsync(Guid id, bool isBlocked, bool? isPermanent, DateTime? blockedUntil, string? reason);
    }
}