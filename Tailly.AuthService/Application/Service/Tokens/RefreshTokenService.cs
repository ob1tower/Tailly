using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Infrastructure.Configurations.Options;

namespace Tailly.AuthService.Application.Service.Tokens;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly JwtOptions _options;
    public RefreshTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string rawToken, string hashedToken) GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var raw = Base64UrlEncoder.Encode(bytes);

        using var sha = SHA256.Create();
        var hashed = Convert.ToBase64String(sha.ComputeHash(bytes));

        return (raw, hashed);
    }

    public string HashToken(string rawToken)
    {
        var bytes = Base64UrlEncoder.DecodeBytes(rawToken);

        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    public DateTime GetRefreshTokenExpiryDate()
    {
        return DateTime.UtcNow.AddDays(_options.RefreshExpiresDays);
    }
}