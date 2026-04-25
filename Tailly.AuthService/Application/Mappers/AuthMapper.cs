using Tailly.AuthService.Application.Dtos.Common;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Mappers;

public static class AuthMapper
{
    public static AuthUserDto ToDto(User user, RoleType role)
    {
        return new AuthUserDto
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            Role = MapRole(role),
            SpecialistId = role == RoleType.Specialist
                ? user.SpecialistId?.ToString()
                : null,
            SpecialistSlug = role == RoleType.Specialist
                ? user.SpecialistSlug
                : null,
            AdminId = role == RoleType.Admin || role == RoleType.SuperAdmin
                ? user.AdminId?.ToString()
                : null
        };
    }

    public static bool TryParseRole(string requestedRole, out RoleType role)
    {
        switch (requestedRole?.Trim().ToLowerInvariant())
        {
            case "client":
                role = RoleType.Client;
                return true;
            case "specialist":
                role = RoleType.Specialist;
                return true;
            case "admin":
                role = RoleType.Admin;
                return true;
            case "super_admin":
                role = RoleType.SuperAdmin;
                return true;
            default:
                role = default;
                return false;
        }
    }

    public static string MapRole(RoleType role) => role switch
    {
        RoleType.Client => "client",
        RoleType.Specialist => "specialist",
        RoleType.Admin => "admin",
        RoleType.SuperAdmin => "super_admin",
        _ => role.ToString().ToLowerInvariant()
    };
}