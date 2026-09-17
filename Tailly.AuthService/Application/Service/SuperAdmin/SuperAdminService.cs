using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Helpers;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.Security.Cryptography;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.SuperAdmin;

public class SuperAdminService : ISuperAdminService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IAdminProfileRepository _adminProfileRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAdminPasswordRecoveryRepository _passwordRecoveryRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SuperAdminService> _logger;

    public SuperAdminService(IUsersRepository usersRepository,
                             IAdminProfileRepository adminProfileRepository,
                             IRefreshTokenRepository refreshTokenRepository,
                             IAdminPasswordRecoveryRepository passwordRecoveryRepository,
                             IPasswordHashingService passwordHasher,
                             IPublishEndpoint publishEndpoint,
                             ILogger<SuperAdminService> logger)
    {
        _usersRepository = usersRepository;
        _adminProfileRepository = adminProfileRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordRecoveryRepository = passwordRecoveryRepository;
        _passwordHasher = passwordHasher;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<ManagedAdminResult, Error>> GetAllAsync(int page, int pageSize)
    {
        var users = await _usersRepository.GetAdminsWithProfilesAsync(page, pageSize);
        var total = await _usersRepository.CountAdminsAsync();

        var items = users.Select(dto =>
        {
            var roleType = (RoleType)dto.Role.RoleId;

            return new ManagedAdmin
            {
                Id = dto.User.Id,
                AdminId = dto.User.AdminId ?? Guid.Empty,
                Email = dto.User.Email ?? string.Empty,
                FirstName = dto.User.FirstName ?? string.Empty,
                LastName = dto.User.LastName ?? string.Empty,
                MiddleName = dto.User.MiddleName ?? string.Empty,

                BirthDate = dto.Profile?.BirthDate,
                Phone = dto.Profile?.Phone ?? string.Empty,
                Department = AuthMapper.MapDepartment(dto.Profile?.Department) ?? string.Empty,

                Role = roleType == RoleType.SuperAdmin ? "super_admin" : "admin",
                Status = dto.Role.IsEffectivelyBlocked ? "inactive" : "active",

                CreatedAt = dto.User.CreatedAt,
                LastLoginAt = dto.Profile?.LastLoginAt,

                IsBlocked = dto.Role.IsBlocked,
                BlockReason = dto.Role.BlockReason ?? string.Empty,
                BlockedUntil = dto.Role.BlockedUntil,
                IsPermanentBlock = dto.Role.IsPermanentBlock,

                PasswordAttemptsLockUntil = dto.Profile?.PasswordAttemptsLockUntil,
                FailedPasswordAttempts = dto.Profile?.FailedPasswordAttempts ?? 0
            };
        }).ToList();

        return Result.Success<ManagedAdminResult, Error>(new ManagedAdminResult
        {
            Items = items,
            Total = total,
            AdminsCount = total
        });
    }

    public async Task<Result<(ManagedAdmin admin, string tempPassword), Error>> CreateAsync(string email, string firstName, string lastName, string? middleName, DateTime birthDate, string? phone, AdminDepartment? department)
    {
        email = email.Trim().ToLowerInvariant();
        if (await _usersRepository.ExistsAsync(email))
            return Result.Failure<(ManagedAdmin, string), Error>(AdminErrors.EmailAlreadyExists);

        var tempPassword = PasswordGenerator.Generate();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var normalizedBirthDate = DateTimeHelper.NormalizeToUtc(birthDate);

        var user = new User
        {
            Id = userId,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(tempPassword),
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            AdminId = adminId,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await _usersRepository.AddAsync(user);          
        await _usersRepository.AddRoleAsync(user.Id, (int)RoleType.Admin);

        var adminProfile = new AdminProfileEntity
        {
            Id = adminId,
            UserId = user.Id,
            BirthDate = normalizedBirthDate,
            Phone = phone,
            Department = department
        };

        await _adminProfileRepository.AddAsync(adminProfile);

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Your admin account has been created",
            Body = $"""
            <h2>Admin account created</h2>
            <p>Email: {user.Email}</p>
            <p>Temporary password: <strong>{tempPassword}</strong></p>
            <p>Please change your password after first login.</p>
            """,
            Purpose = "admin-created"
        });

        _logger.LogInformation("SuperAdmin created new admin. AdminId={AdminId}, UserId={UserId}", adminId, userId);

        var managedAdmin = new ManagedAdmin
        {
            Id = user.Id,
            AdminId = adminId,
            Email = user.Email,
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName ?? string.Empty,
            BirthDate = normalizedBirthDate,
            Phone = phone ?? string.Empty,
            Department = AuthMapper.MapDepartment(department) ?? string.Empty,
            Role = "admin",
            Status = "active",
            CreatedAt = user.CreatedAt,
            IsBlocked = false
        };

        return Result.Success<(ManagedAdmin admin, string tempPassword), Error>((managedAdmin, tempPassword));
    }

    public async Task<Result<bool, Error>> DeleteAsync(Guid adminId)
    {
        var user = await _usersRepository.GetAdminByAdminIdAsync(adminId);
        if (user == null)
        {
            _logger.LogWarning("Admin not found for deletion. AdminId={AdminId}", adminId);
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);
        }

        var adminRole = user.GetRole(RoleType.Admin);
        var superAdminRole = user.GetRole(RoleType.SuperAdmin);

        if (adminRole == null && superAdminRole == null)
        {
            _logger.LogWarning("User {UserId} is not an admin", user.Id);
            return Result.Failure<bool, Error>(AdminErrors.InvalidRole);
        }

        var profile = await _adminProfileRepository.GetByUserIdAsync(user.Id);
        if (profile != null)
            await _adminProfileRepository.DeleteAsync(profile.Id);

        await _usersRepository.DeleteUserRolesAsync(user.Id);

        await _usersRepository.DeleteUserAsync(user.Id);

        await _refreshTokenRepository.InvalidateAllAsync(user.Id);

        _logger.LogInformation("Admin permanently deleted. AdminId={AdminId}, UserId={UserId}", adminId, user.Id);
        return Result.Success<bool, Error>(true);
    }

    public async Task<Result<ManagedAdmin, Error>> UpdateAsync(Guid adminId, string firstName, string lastName, string? middleName, DateTime? birthDate, string? phone, AdminDepartment? department)
    {
        var user = await _usersRepository.GetAdminByAdminIdAsync(adminId);
        if (user == null)
        {
            _logger.LogWarning("Admin not found for update. AdminId={AdminId}", adminId);
            return Result.Failure<ManagedAdmin, Error>(AdminErrors.UserNotFound);
        }

        var role = user.GetRole(RoleType.Admin);
        if (role == null || role.SoftDeletedAt != null)
        {
            _logger.LogWarning("Invalid admin role for update. AdminId={AdminId}", adminId);
            return Result.Failure<ManagedAdmin, Error>(AdminErrors.InvalidRole);
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.MiddleName = middleName;
        await _usersRepository.UpdateAsync(user);

        var profile = await _adminProfileRepository.GetByUserIdAsync(user.Id);

        if (profile == null)
        {
            _logger.LogWarning("AdminProfile not found for user {UserId} during update", user.Id);
        }
        else
        {
            profile.Phone = phone;
            profile.Department = department;

            if (birthDate.HasValue)
            {
                profile.BirthDate = DateTimeHelper.NormalizeToUtc(birthDate.Value);
            }

            await _adminProfileRepository.UpdateAsync(profile);
        }

        _logger.LogInformation("Admin updated successfully. AdminId={AdminId}", adminId);

        var updatedProfile = await _adminProfileRepository.GetByUserIdAsync(user.Id);

        var result = new ManagedAdmin
        {
            Id = user.Id,
            AdminId = user.AdminId ?? Guid.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            MiddleName = user.MiddleName ?? string.Empty,
            BirthDate = updatedProfile?.BirthDate,
            Phone = updatedProfile?.Phone ?? string.Empty,
            Department = AuthMapper.MapDepartment(updatedProfile?.Department) ?? string.Empty,

            Role = "admin",
            Status = role.IsEffectivelyBlocked ? "inactive" : "active",

            CreatedAt = user.CreatedAt,
            IsBlocked = role.IsBlocked,
            BlockReason = role.BlockReason ?? string.Empty,
            BlockedUntil = role.BlockedUntil,
            IsPermanentBlock = role.IsPermanentBlock
        };

        return Result.Success<ManagedAdmin, Error>(result);
    }

    public async Task<Result<bool, Error>> UpdateBlockStatusAsync(Guid adminId, bool isBlocked, string? blockReason = null, DateTime? blockedUntil = null, bool? isPermanentBlock = null)
    {
        var user = await _usersRepository.GetAdminByAdminIdAsync(adminId);
        if (user == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);

        var adminRole = user.GetRole(RoleType.Admin);
        if (adminRole == null)
            return Result.Failure<bool, Error>(AdminErrors.InvalidRole);

        if (!isBlocked)
        {
            await _usersRepository.PatchUserRoleBlockAsync(user.Id, RoleType.Admin, false, false, null, null);
            await _refreshTokenRepository.InvalidateAllAsync(user.Id);
            return Result.Success<bool, Error>(true);
        }

        var permanent = isPermanentBlock ?? false;
        DateTime? finalBlockedUntil = null;

        if (!permanent && blockedUntil.HasValue)
        {
            var until = DateTimeHelper.NormalizeToUtc(blockedUntil.Value);
            if (until <= DateTime.UtcNow)
                return Result.Failure<bool, Error>(AdminErrors.InvalidBlockDate);
            finalBlockedUntil = until;
        }

        await _usersRepository.PatchUserRoleBlockAsync(
            user.Id,
            RoleType.Admin,
            true,
            permanent,
            finalBlockedUntil,
            blockReason);

        await _refreshTokenRepository.InvalidateAllAsync(user.Id);

        _logger.LogInformation("Admin blocked. AdminId={AdminId}, Reason={Reason}", adminId, blockReason);

        return Result.Success<bool, Error>(true);
    }

    public async Task<Result<bool, Error>> ClearPasswordAttemptsLockAsync(Guid adminId)
    {
        var user = await _usersRepository.GetAdminByAdminIdAsync(adminId);
        if (user == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);

        var profile = await _adminProfileRepository.GetByUserIdAsync(user.Id);
        if (profile == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);

        profile.PasswordAttemptsLockUntil = null;
        profile.FailedPasswordAttempts = 0;

        await _adminProfileRepository.UpdateAsync(profile);

        _logger.LogInformation("Password attempts lock cleared for admin. AdminId={AdminId}", adminId);

        return Result.Success<bool, Error>(true);
    }

    public async Task<Result<List<AdminPasswordRecovery>, Error>> GetPasswordRecoveryAsync()
    {
        var requests = await _passwordRecoveryRepository.GetAllAsync();
        return Result.Success<List<AdminPasswordRecovery>, Error>(requests); 
    }

    public async Task<Result<(string temporaryPassword, string email), Error>> ProcessPasswordRecoveryAsync(Guid requestId)
    {
        var request = await _passwordRecoveryRepository.GetByIdAsync(requestId);
        if (request == null)
            return Result.Failure<(string, string), Error>(AdminErrors.UserNotFound);

        if (request.Status == AdminPasswordRecoveryStatus.Processed)
            return Result.Failure<(string, string), Error>(AdminErrors.RequestAlreadyProcessed);

        var tempPassword = PasswordGenerator.Generate();

        var user = await _usersRepository.GetByEmailAsync(request.Email);
        if (user != null)
        {
            user.PasswordHash = _passwordHasher.HashPassword(tempPassword);
            await _usersRepository.UpdateAsync(user);
            await _refreshTokenRepository.InvalidateAllAsync(user.Id); 
        }

        request.Status = AdminPasswordRecoveryStatus.Processed;
        request.ProcessedAt = DateTimeHelper.NormalizeToUtc(DateTime.UtcNow);
        request.TemporaryPassword = tempPassword;

        await _passwordRecoveryRepository.UpdateAsync(request);

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = request.Email,
            Subject = "Your password has been reset",
            Body = $"""
            <h2>Password Reset</h2>
            <p>The administrator has reset your password.</p>
            <p><strong>New temporary password:</strong> {tempPassword}</p>
            <p>Please log in and change your password as soon as possible.</p>
            """,
            Purpose = "admin-password-reset"
        });

        _logger.LogInformation("Password recovery request processed. RequestId={RequestId}, Email={Email}", requestId, request.Email);
        return Result.Success<(string, string), Error>((tempPassword, request.Email));
    }
}