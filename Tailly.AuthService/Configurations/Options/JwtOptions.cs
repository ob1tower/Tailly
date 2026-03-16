namespace Tailly.AuthService.Configurations.Options;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = default!;

    public string Audience { get; set; } = default!;

    public string Key { get; set; } = default!;

    public int AccessExpiresMinutes { get; set; }
}
