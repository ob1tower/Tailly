using System.Security.Cryptography;

namespace Tailly.AuthService.Application.Service.Security.Cryptography;

public static class PasswordGenerator
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

    public static string Generate(int length = 12)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        return new string(bytes.Select(b => Chars[b % Chars.Length]).ToArray());
    }
}