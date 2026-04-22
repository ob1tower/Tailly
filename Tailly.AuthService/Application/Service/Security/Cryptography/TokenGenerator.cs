using System.Security.Cryptography;

namespace Tailly.AuthService.Application.Service.Security.Cryptography;

public static class TokenGenerator
{
    public static string Generate()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
}