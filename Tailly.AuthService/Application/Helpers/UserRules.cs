using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Helpers;

public static class UserRules
{
    public static bool IsPasswordRecoveryBlockedForAllRoles(User user)
    {
        if (user.UserRoles.Any(x =>
            x.SoftDeletedAt == null &&
            (x.Role == RoleType.Admin || x.Role == RoleType.SuperAdmin)))
        {
            var adminRole = user.UserRoles.FirstOrDefault(x =>
                x.SoftDeletedAt == null &&
                (x.Role == RoleType.Admin || x.Role == RoleType.SuperAdmin));

            return adminRole?.IsEffectivelyBlocked == true;
        }

        var relevantRoles = user.UserRoles
            .Where(x => x.SoftDeletedAt == null &&
                       (x.Role == RoleType.Client || x.Role == RoleType.Specialist))
            .ToList();

        if (relevantRoles.Count == 0)
            return false;

        return relevantRoles.All(r => r.IsEffectivelyBlocked);
    }
}