using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.Admin
{
    public interface IAdminUserService
    {
        Task<Result<AdminUserResult, Error>> GetAllAsync(string? search, string? role, int page, int pageSize);
        Task<Result<AdminUser, Error>> GetByIdAsync(Guid id, string roleScope);
        Task<Result<bool, Error>> RestoreFromDeletionAsync(Guid id, string roleScope);
        Task<Result<bool, Error>> UpdateBlockStatusAsync(Guid userId, RoleType role, bool isBlocked, bool? isPermanent, DateTime? blockedUntil, string? reason);
        Task<Result<bool, Error>> UpdateProfileAsync(Guid id, string firstName, string lastName, string? middleName, string? specialistSlug);
    }
}