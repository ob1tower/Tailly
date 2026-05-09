namespace Tailly.BookingService.Infrastructure.Configurations.Options;

public sealed class JwtOptions
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SecretKey { get; set; }
}