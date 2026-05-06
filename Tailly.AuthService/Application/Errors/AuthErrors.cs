using Tailly.AuthService.Core.Common;

namespace Tailly.AuthService.Application.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Invalid email or password.");

    public static readonly Error InvalidRole =
        new("Auth.InvalidRole", "User does not have access to this role.");

    public static readonly Error AccountBlocked =
        new("Auth.AccountBlocked", "Account is blocked.");

    public static readonly Error AccountPendingDeletion =
        new("Auth.AccountPendingDeletion", "Account is pending deletion.");

    public static readonly Error EmailNotConfirmed =
        new("Auth.EmailNotConfirmed", "Email is not confirmed. Please verify your email first.");

    public static readonly Error UserAlreadyExists =
        new("Auth.UserAlreadyExists", "User with this email already exists.");

    public static readonly Error RegistrationNotFound =
        new("Auth.RegistrationNotFound", "Registration session not found.");

    public static readonly Error InvalidVerificationToken =
        new("Auth.InvalidVerificationToken", "Verification token is invalid or expired.");

    public static readonly Error InvalidVerificationCode =
        new("Auth.InvalidVerificationCode", "The code is incorrect, expired, or the number of attempts exceeded. Request a new code.");

    public static readonly Error SamePassword =
        new("Auth.SamePassword", "New password must be different from current password.");

    public static readonly Error InvalidRefreshToken =
        new("Auth.InvalidRefreshToken", "Invalid refresh token.");

    public static readonly Error InvalidPassword =
        new("Auth.InvalidPassword", "Current password is incorrect.");

    public static readonly Error SameEmail =
        new("Auth.SameEmail", "New email must be different from current email.");

    public static readonly Error AccessDenied =
        new("Auth.AccessDenied", "You do not have permission to perform this action.");

    public static readonly Error UserNotFound =
        new("Auth.UserNotFound", "User not found.");

    public static readonly Error RoleNotFoundForUser =
        new("Auth.RoleNotFoundForUser", "Specified role not found for this user.");

    public static readonly Error DeletionUpdateFailed =
        new("Auth.DeletionUpdateFailed", "Failed to update deletion status.");

    public static readonly Error AccountTemporarilyLocked =
        new("Auth.AccountTemporarilyLocked", "Your account is temporarily locked due to too many failed login attempts. Please try again later.");
}