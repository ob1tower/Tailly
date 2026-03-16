namespace Tailly.AuthService.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Invalid email or password");

    public static readonly Error UserAlreadyExists =
        new("Auth.UserAlreadyExists", "User with this email already exists for this role");

    public static readonly Error InvalidRefreshToken =
        new("Auth.InvalidRefreshToken", "Invalid refresh token");

    public static readonly Error RefreshTokenExpired =
        new("Auth.RefreshTokenExpired", "Refresh token expired");

    public static readonly Error TokenRevoked =
        new("Auth.TokenRevoked", "Token has been revoked");

    public static readonly Error InvalidRole =
        new("Auth.InvalidRole", "Invalid role");

    public static readonly Error SpecialistPending =
        new("Auth.SpecialistPending", "Specialist application is pending approval");
}