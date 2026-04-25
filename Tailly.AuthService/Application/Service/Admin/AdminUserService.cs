using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Helpers;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;

namespace Tailly.AuthService.Application.Service.Admin;

public class AdminUserService : IAdminUserService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(IUsersRepository usersRepository,
                            IRefreshTokenRepository refreshTokenRepository,
                            IPublishEndpoint publishEndpoint,
                            ILogger<AdminUserService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<AdminUserResult, Error>> GetAllAsync(string? search, string? roleFilter, int page, int pageSize)
    {
        var query = _usersRepository.Query()
            .SelectMany(u => u.UserRoles, (user, userRole) => new { User = user, UserRole = userRole })
            .Where(x =>
                x.UserRole.SoftDeletedAt == null &&                    
                (x.UserRole.RoleId == (int)RoleType.Client ||
                 x.UserRole.RoleId == (int)RoleType.Specialist));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLowerInvariant();
            query = query.Where(x =>
                (x.User.Email != null && x.User.Email.ToLower().Contains(searchTerm)) ||
                (x.User.FirstName != null && x.User.FirstName.ToLower().Contains(searchTerm)) ||
                (x.User.LastName != null && x.User.LastName.ToLower().Contains(searchTerm)) ||
                (x.User.SpecialistSlug != null && x.User.SpecialistSlug.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            roleFilter = roleFilter.ToLowerInvariant();
            if (roleFilter == "client")
                query = query.Where(x => x.UserRole.RoleId == (int)RoleType.Client);
            else if (roleFilter == "specialist")
                query = query.Where(x => x.UserRole.RoleId == (int)RoleType.Specialist);
        }

        var baseQuery = _usersRepository.Query();

        var clientsCount = await baseQuery
            .CountAsync(u => u.UserRoles.Any(r =>
                r.RoleId == (int)RoleType.Client && r.SoftDeletedAt == null));

        var specialistsCount = await baseQuery
            .CountAsync(u => u.UserRoles.Any(r =>
                r.RoleId == (int)RoleType.Specialist && r.SoftDeletedAt == null));

        var total = await query.CountAsync();

        var rows = await query
            .OrderBy(x => x.User.LastName)
            .ThenBy(x => x.User.Email)
            .ThenBy(x => x.UserRole.RoleId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rows.Select(x => new AdminUser
        {
            Id = x.User.Id,
            Email = x.User.Email,

            Role = AuthMapper.MapRole((RoleType)x.UserRole.RoleId),

            IsBlocked = x.UserRole.IsBlocked,
            IsPermanentBlock = x.UserRole.IsPermanentBlock,
            BlockedUntil = x.UserRole.BlockedUntil,
            BlockReason = x.UserRole.BlockReason,

            SoftDeletedAt = x.UserRole.SoftDeletedAt,
            RestoreUntil = x.UserRole.RestoreUntil,

            FirstName = x.User.FirstName,
            LastName = x.User.LastName,
            MiddleName = x.User.MiddleName,

            SpecialistSlug = x.UserRole.RoleId == (int)RoleType.Specialist ? x.User.SpecialistSlug : null,
            SpecialistId = x.UserRole.RoleId == (int)RoleType.Specialist ? x.User.SpecialistId : null,

            AdminId = x.User.AdminId
        }).ToList();

        return Result.Success<AdminUserResult, Error>(new AdminUserResult
        {
            Items = items,
            Total = total,
            ClientsCount = clientsCount,
            SpecialistsCount = specialistsCount
        });
    }

    public async Task<Result<AdminUser, Error>> GetByIdAsync(Guid id, string roleScope)
    {
        if (string.IsNullOrWhiteSpace(roleScope))
            return Result.Failure<AdminUser, Error>(AdminErrors.InvalidRole);

        if (!AuthMapper.TryParseRole(roleScope, out var roleType))
            return Result.Failure<AdminUser, Error>(AdminErrors.InvalidRole);

        var user = await _usersRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User not found {UserId}", id);
            return Result.Failure<AdminUser, Error>(AdminErrors.UserNotFound);
        }

        var userRole = user.GetRole(roleType);
        if (userRole == null || userRole.SoftDeletedAt != null)
        {
            _logger.LogWarning("Role {Role} not found or soft-deleted for user {UserId}", roleType, id);
            return Result.Failure<AdminUser, Error>(AdminErrors.UserRoleNotFound);
        }

        var adminUser = new AdminUser
        {
            Id = user.Id,
            Email = user.Email,
            Role = AuthMapper.MapRole(roleType),       

            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,

            SpecialistSlug = roleType == RoleType.Specialist ? user.SpecialistSlug : null,
            SpecialistId = roleType == RoleType.Specialist ? user.SpecialistId : null,
            AdminId = user.AdminId,

            IsBlocked = userRole.IsBlocked,
            IsPermanentBlock = userRole.IsPermanentBlock,
            BlockedUntil = userRole.BlockedUntil,
            BlockReason = userRole.BlockReason,
            SoftDeletedAt = userRole.SoftDeletedAt,
            RestoreUntil = userRole.RestoreUntil,
        };

        return Result.Success<AdminUser, Error>(adminUser);
    }

    public async Task<Result<bool, Error>> UpdateBlockStatusAsync(Guid userId, RoleType role, bool isBlocked, bool? isPermanent, DateTime? blockedUntil, string? reason)
    {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);

        var userRole = user.GetRole(role);
        if (userRole == null || userRole.SoftDeletedAt != null)
            return Result.Failure<bool, Error>(AdminErrors.InvalidRole);

        if (!isBlocked)
        {
            await _usersRepository.PatchUserRoleBlockAsync(userId, role, false, false, null, null);

            await _refreshTokenRepository.InvalidateAllForUserAndRoleAsync(userId, (int)role);

            return Result.Success<bool, Error>(true);
        }

        var permanent = isPermanent ?? false;

        DateTime? finalBlockedUntil = null;

        if (!permanent)
        {
            if (!blockedUntil.HasValue)
                return Result.Failure<bool, Error>(AdminErrors.BlockDateRequired);

            var until = DateTimeHelper.NormalizeToUtc(blockedUntil.Value);

            if (until <= DateTime.UtcNow)
                return Result.Failure<bool, Error>(AdminErrors.InvalidBlockDate);

            finalBlockedUntil = until;
        }

        await _usersRepository.PatchUserRoleBlockAsync(userId, role, isBlocked, permanent, finalBlockedUntil, reason);

        await _refreshTokenRepository.InvalidateAllForUserAndRoleAsync(userId, (int)role);

        if (isBlocked)
        {
            await _publishEndpoint.Publish(new SendEmailMessage
            {
                To = user.Email,
                Subject = "Your account is blocked",
                Body = $"""
                <h2>Account is blocked</h2>
                <p>Role: {role}</p>
                <p>Reason: {reason ?? "Not specified"}</p>
                """,
                Purpose = "account-blocked"
            });
        }

        return Result.Success<bool, Error>(true);
    }

    public async Task<Result<bool, Error>> RestoreFromDeletionAsync(Guid id, string roleScope)
    {
        if (!Enum.TryParse<RoleType>(roleScope, true, out var role))
            return Result.Failure<bool, Error>(AdminErrors.InvalidRole);

        var user = await _usersRepository.GetByIdAsync(id);
        if (user == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);

        var userRole = user.GetRole(role);
        if (userRole == null || userRole.SoftDeletedAt == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotDeleted);

        await _usersRepository.PatchUserRoleSoftDeleteAsync(id, role, null, null);

        return Result.Success<bool, Error>(true);
    }

    public async Task<Result<bool, Error>> UpdateProfileAsync(Guid id, string firstName, string lastName, string? middleName, string? specialistSlug)
    {
        var user = await _usersRepository.GetByIdAsync(id);
        if (user == null)
            return Result.Failure<bool, Error>(AdminErrors.UserNotFound);

        user.FirstName = firstName;
        user.LastName = lastName;
        user.MiddleName = middleName;

        if (specialistSlug != null)
        {
            var isSpecialist = user.UserRoles.Any(x =>
                x.Role == RoleType.Specialist && x.SoftDeletedAt == null);

            if (!isSpecialist)
                return Result.Failure<bool, Error>(AdminErrors.SpecialistOnlyField);

            user.SpecialistSlug = specialistSlug;
        }

        await _usersRepository.UpdateAsync(user);

        return Result.Success<bool, Error>(true);
    }
}