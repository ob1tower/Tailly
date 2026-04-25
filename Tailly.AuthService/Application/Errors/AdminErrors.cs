using Tailly.AuthService.Core.Common;

namespace Tailly.AuthService.Application.Errors;

public static class AdminErrors
{
    public static readonly Error UserNotFound =
        new("Admin.UserNotFound", "User not found.");

    public static readonly Error UserAlreadyBlocked =
        new("Admin.UserAlreadyBlocked", "User already blocked.");

    public static readonly Error UserNotBlocked =
        new("Admin.UserNotBlocked", "User is not blocked.");

    public static readonly Error InvalidBlockDate =
        new("Admin.InvalidBlockDate", "BlockedUntil must be in the future.");

    public static readonly Error BlockDateRequired =
        new("Admin.BlockDateRequired", "BlockedUntil is required.");

    public static readonly Error UserAlreadyDeleted =
        new("Admin.UserAlreadyDeleted", "User already deleted.");

    public static readonly Error UserNotDeleted =
        new("Admin.UserNotDeleted", "User is not deleted.");

    public static readonly Error InvalidRole =
        new("Admin.InvalidRole", "Invalid role specified for this operation.");

    public static readonly Error SpecialistOnlyField =
        new("Admin.SpecialistOnlyField", "Specialist slug can only be updated for users with Specialist role.");

    public static readonly Error InvalidManagedRole =
        new("Admin.InvalidManagedRole", "Invalid role scope. Allowed: client, specialist.");

    public static readonly Error UpdateFailed =
        new("Admin.UpdateFailed", "Failed to update user data.");

    public static readonly Error UserRoleNotFound =
        new("Admin.UserRoleNotFound", "Specified role not found for this user or has been deleted.");
}