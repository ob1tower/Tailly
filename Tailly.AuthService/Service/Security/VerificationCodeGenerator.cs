using System.Security.Cryptography;

namespace Tailly.AuthService.Service.Security;

public static class VerificationCodeGenerator
{
    private const int MinValue = 100000;
    private const int MaxValue = 999999;

    public static string GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(4);
        uint value = BitConverter.ToUInt32(bytes, 0);

        int code = MinValue + (int)(value % (MaxValue - MinValue + 1));

        return code.ToString("D6");
    }
}
