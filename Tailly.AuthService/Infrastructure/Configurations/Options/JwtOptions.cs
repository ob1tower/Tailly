namespace Tailly.AuthService.Infrastructure.Configurations.Options;

public sealed class JwtOptions
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SecretKey { get; set; }
    public required int AccessExpiresMinutes { get; set; }
    public required int RefreshExpiresDays { get; set; }
}