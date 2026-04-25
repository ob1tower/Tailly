using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Helpers;

public static class SessionRole
{
    public static bool TryResolveSessionRole(RefreshToken storedToken, User user, out RoleType sessionRole)
    {
        if (storedToken.RoleId.HasValue)
        {
            sessionRole = (RoleType)storedToken.RoleId.Value;
            return true;
        }

        if (user.Roles.Count == 1)
        {
            sessionRole = user.Roles[0];
            return true;
        }

        sessionRole = default;
        return false;
    }
}