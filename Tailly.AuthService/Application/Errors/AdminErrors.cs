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
}