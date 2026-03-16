using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Tailly.AuthService.Service.Tokens;

public class RefreshTokenService : IRefreshTokenService
{
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
}
