using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.Admin;

public class AdminUserService : IAdminUserService
{
    private readonly IUsersRepository _usersRepository;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(IUsersRepository usersRepository,
                            ILogger<AdminUserService> logger)
    {
        _usersRepository = usersRepository;
        _logger = logger;
    }

    public async Task<Result<AdminUserResult, Error>> GetAllAsync(string? search, string? role, int page, int pageSize)
    {
        var query = _usersRepository.Query();

        query = query.Where(u => u.UserRoles.Any(r =>
        r.RoleId == (int)RoleType.Client ||
        r.RoleId == (int)RoleType.Specialist));

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();

            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                (u.SpecialistSlug != null && u.SpecialistSlug.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            if (role == "client")
                query = query.Where(u =>
                    u.UserRoles.Any(r => r.RoleId == (int)RoleType.Client));

            if (role == "specialist")
                query = query.Where(u =>
                    u.UserRoles.Any(r => r.RoleId == (int)RoleType.Specialist));
        }

        var baseQuery = _usersRepository.Query();

        var clientsCount = await baseQuery.CountAsync(u =>
            u.UserRoles.Any(r => r.RoleId == (int)RoleType.Client));

        var specialistsCount = await baseQuery.CountAsync(u =>
            u.UserRoles.Any(r => r.RoleId == (int)RoleType.Specialist));

        var total = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = users.Select(user => new AdminUser
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.UserRoles.Any(r => r.RoleId == (int)RoleType.Specialist)
                ? "specialist"
                : "client",

            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            SpecialistSlug = user.SpecialistSlug,

            IsBlocked = user.IsBlocked,
            IsPermanentBlock = user.IsPermanentBlock,
            BlockedUntil = user.BlockedUntil,
            BlockReason = user.BlockReason,

            SpecialistId = user.SpecialistId,
            AdminId = user.AdminId
        }).ToList();

        return Result.Success<AdminUserResult, Error>(new AdminUserResult
        {
            Items = items,
            Total = total,
            ClientsCount = clientsCount,
            SpecialistsCount = specialistsCount
        });
    }

    public async Task<Result<AdminUser, Error>> GetByIdAsync(Guid id)
    {
        var user = await _usersRepository.GetByIdAsync(id);

        if (user == null)
        {
            _logger.LogWarning("User not found {UserId}", id);
            return Result.Failure<AdminUser, Error>(AdminErrors.UserNotFound);
        }

        var result = new AdminUser
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Roles.Contains(RoleType.Specialist)
                ? "specialist"
                : "client",

            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            SpecialistSlug = user.SpecialistSlug,

            IsBlocked = user.IsBlocked,
            IsPermanentBlock = user.IsPermanentBlock,
            BlockedUntil = user.BlockedUntil,
            BlockReason = user.BlockReason,

            SoftDeletedAt = user.SoftDeletedAt,
            RestoreUntil = user.RestoreUntil,

            SpecialistId = user.SpecialistId,
            AdminId = user.AdminId
        };

        return Result.Success<AdminUser, Error>(result);
    }

    public async Task<Result<bool, Error>> UpdateBlockStatusAsync(Guid id, bool isBlocked, bool? isPermanent, DateTime? blockedUntil, string? reason)
    {
        var user = await _usersRepository.GetByIdAsync(id);

        if (user == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);

        if (!isBlocked)
        {
            user.IsBlocked = false;
            user.IsPermanentBlock = false;
            user.BlockedUntil = null;
            user.BlockReason = null;

            await _usersRepository.UpdateAsync(user);
            return Result.Success<bool, Error>(true);
        }

        var permanent = isPermanent ?? false;

        if (!permanent)
        {
            if (!blockedUntil.HasValue)
                return Result.Failure<bool, Error>(AdminErrors.BlockDateRequired);

            if (blockedUntil.Value <= DateTime.UtcNow)
                return Result.Failure<bool, Error>(AdminErrors.InvalidBlockDate);
        }

        user.IsBlocked = true;
        user.IsPermanentBlock = permanent;
        user.BlockedUntil = permanent ? null : blockedUntil;
        user.BlockReason = reason;

        await _usersRepository.UpdateAsync(user);
        return Result.Success<bool, Error>(true);
    }

    public async Task<Result<bool, Error>> RestoreFromDeletionAsync(Guid id)
    {
        var user = await _usersRepository.GetByIdAsync(id);

        if (user == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);

        if (user.SoftDeletedAt == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotDeleted);

        user.SoftDeletedAt = null;
        user.RestoreUntil = null;

        await _usersRepository.UpdateAsync(user);

        return Result.Success<bool, Error>(true);
    }
}