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
}