using CSharpFunctionalExtensions;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Helpers;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.AdminProfiles;

public class AdminProfileService : IAdminProfileService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IAdminProfileRepository _adminProfileRepository;
    private readonly ILogger<AdminProfileService> _logger;

    public AdminProfileService(IUsersRepository usersRepository,
                               IAdminProfileRepository adminProfileRepository,
                               ILogger<AdminProfileService> logger)
    {
        _usersRepository = usersRepository;
        _adminProfileRepository = adminProfileRepository;
        _logger = logger;
    }

    public async Task<Result<AdminProfile, Error>> GetAsync(Guid userId)
    {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure<AdminProfile, Error>(AdminErrors.UserNotFound);

        var profile = await _adminProfileRepository.GetByUserIdAsync(userId);

        var isSuperAdmin = user.UserRoles.Any(r => r.Role == RoleType.SuperAdmin);

        var adminProfile = new AdminProfile
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            MiddleName = user.MiddleName,
            BirthDate = profile?.BirthDate,
            Phone = profile?.Phone,
            Department = AuthMapper.MapDepartment(profile?.Department),
            LastLoginAt = profile?.LastLoginAt,
            Role = isSuperAdmin ? "super_admin" : "admin"
        };

        return Result.Success<AdminProfile, Error>(adminProfile);
    }

    public async Task<Result<AdminProfile, Error>> UpdateAsync(Guid userId, string firstName, string lastName, string? middleName, DateTime? birthDate, string? phone, AdminDepartment? department)
    {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure<AdminProfile, Error>(AdminErrors.UserNotFound);

        var isSuperAdmin = user.UserRoles.Any(r =>
            r.Role == RoleType.SuperAdmin && r.SoftDeletedAt == null);

        if (!isSuperAdmin && birthDate.HasValue)
        {
            return Result.Failure<AdminProfile, Error>(AdminErrors.BirthDateNotAllowed);
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.MiddleName = middleName;

        await _usersRepository.UpdateAsync(user);

        var profile = await _adminProfileRepository.GetByUserIdAsync(userId);
        if (profile != null)
        {
            profile.Phone = phone;
            profile.Department = department;

            if (birthDate.HasValue)
                profile.BirthDate = DateTimeHelper.NormalizeToUtc(birthDate.Value);

            await _adminProfileRepository.UpdateAsync(profile);
        }

        _logger.LogInformation("Admin profile updated. UserId={UserId}", userId);

        return await GetAsync(userId);
    }
}