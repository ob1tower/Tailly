using CSharpFunctionalExtensions;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Service.AdminProfiles
{
    public interface IAdminProfileService
    {
        Task<Result<AdminProfile, Error>> GetAsync(Guid userId);
        Task<Result<AdminProfile, Error>> UpdateAsync(Guid userId, string firstName, string lastName, string? middleName, DateTime? birthDate, string? phone, AdminDepartment? department);
    }
}