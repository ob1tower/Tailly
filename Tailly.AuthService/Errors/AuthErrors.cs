namespace Tailly.AuthService.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Invalid email or password.");

    public static readonly Error UserAlreadyExists =
        new("Auth.UserAlreadyExists", "User with this email already exists.");

    public static readonly Error InvalidRefreshToken =
        new("Auth.InvalidRefreshToken", "Invalid refresh token.");
}