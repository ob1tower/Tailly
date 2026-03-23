namespace Tailly.AuthService.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Invalid email or password.");

    public static readonly Error UserAlreadyExists =
        new("Auth.UserAlreadyExists", "User with this email already exists.");

    public static readonly Error InvalidRefreshToken =
        new("Auth.InvalidRefreshToken", "Invalid refresh token.");

    public static readonly Error InvalidPassword =
        new("Auth.InvalidPassword", "Current password is incorrect.");

    public static readonly Error SamePassword =
        new("Auth.SamePassword", "New password must be different from current password.");

    public static readonly Error EmailNotConfirmed =
        new("Auth.EmailNotConfirmed", "Email is not confirmed. Please verify your email first.");

    public static readonly Error InvalidVerificationCode = 
        new("Auth.InvalidVerificationCode", "The code is incorrect, expired, or the number of attempts exceeded. Request a new code.");

    public static readonly Error SameEmail =
        new("Auth.SameEmail", "New email must be different from current.");
}