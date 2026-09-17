using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Infrastructure.Mappers;

public static class UserEntityMapper
{
    public static User ToDomain(UserEntity userEntity)
    {
        return new User
        {
            Id = userEntity.Id,
            Email = userEntity.Email,
            PasswordHash = userEntity.PasswordHash,
            CreatedAt = userEntity.CreatedAt,
            EmailConfirmed = userEntity.EmailConfirmed,

            Roles = userEntity.UserRoles
                .Select(x => (RoleType)x.RoleId)
                .ToList(),

            UserRoles = userEntity.UserRoles
                .Select(ur => new UserRole
                {
                    Role = (RoleType)ur.RoleId,
                    IsBlocked = ur.IsBlocked,
                    IsPermanentBlock = ur.IsPermanentBlock,
                    BlockedUntil = ur.BlockedUntil,
                    BlockReason = ur.BlockReason,
                    SoftDeletedAt = ur.SoftDeletedAt,
                    RestoreUntil = ur.RestoreUntil
                })
                .ToList(),

            SpecialistSlug = userEntity.SpecialistSlug,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            MiddleName = userEntity.MiddleName,
            SpecialistId = userEntity.SpecialistId,
            AdminId = userEntity.AdminId
        };
    }
}