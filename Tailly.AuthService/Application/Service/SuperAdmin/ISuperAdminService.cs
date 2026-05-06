using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.SuperAdmin
{
    public interface ISuperAdminService
    {
        Task<Result<(ManagedAdmin admin, string tempPassword), Error>> CreateAsync(string email, string firstName, string lastName, string? middleName, DateTime birthDate, string? phone, AdminDepartment? department);
        Task<Result<bool, Error>> DeleteAsync(Guid adminId);
        Task<Result<bool, Error>> ClearPasswordAttemptsLockAsync(Guid adminId);
        Task<Result<List<AdminPasswordRecovery>, Error>> GetPasswordRecoveryAsync();
        Task<Result<(string temporaryPassword, string email), Error>> ProcessPasswordRecoveryAsync(Guid requestId);
        Task<Result<bool, Error>> UpdateBlockStatusAsync(Guid adminId, bool isBlocked, string? blockReason = null, DateTime? blockedUntil = null, bool? isPermanentBlock = null);
        Task<Result<ManagedAdminResult, Error>> GetAllAsync(int page, int pageSize);
        Task<Result<ManagedAdmin, Error>> UpdateAsync(Guid adminId, string firstName, string lastName, string? middleName, DateTime? birthDate, string? phone, AdminDepartment? department);
    }
}