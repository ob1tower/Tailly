using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Models.Internal;

namespace Tailly.AuthService.Application.Service.Security.Otp;

public class VerificationCodeService : IVerificationCodeService
{
    private readonly IDatabase _redis;
    private readonly string _otpPepper;

    private const string KeyPrefix = "verify:email:";
    private const int MaxAttempts = 8;
    private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(15);

    public VerificationCodeService(IConnectionMultiplexer redis,
                                   IConfiguration config)
    {
        _redis = redis.GetDatabase();
        _otpPepper = config["Security:OtpPepper"]
            ?? throw new InvalidOperationException("OtpPepper not configured.");
    }

    public async Task SetCodeAsync(string email, string code, string purpose = "default")
    {
        var normalizedPurpose = NormalizePurpose(purpose);
        var key = BuildKey(email, normalizedPurpose);

        var entry = new OtpEntry
        {
            CodeHash = ComputeOtpHash(code),
            Attempts = 0,
            Purpose = normalizedPurpose,
            CreatedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(entry);
        await _redis.StringSetAsync(key, json, CodeTtl);
    }

    public async Task<(bool Success, string? Error)> VerifyCodeAsync(string email,string code,
                                                                     string expectedPurpose = "default")
    {
        var normalizedPurpose = NormalizePurpose(expectedPurpose);
        var key = BuildKey(email, normalizedPurpose);

        var value = await _redis.StringGetAsync(key);
        if (value.IsNull)
            return (false, "Code expired or not found.");

        OtpEntry? entry;
        try
        {
            entry = JsonSerializer.Deserialize<OtpEntry>(value!);
        }
        catch (JsonException)
        {
            await _redis.KeyDeleteAsync(key);
            return (false, "Invalid stored data.");
        }

        if (entry == null || string.IsNullOrEmpty(entry.CodeHash))
            return (false, "Invalid stored data.");

        if (entry.Purpose != normalizedPurpose)
            return (false, "Incorrect code assignment.");

        if (entry.Attempts >= MaxAttempts)
            return (false, "Too many attempts. Request a new code.");

        var isValid = VerifyHash(entry.CodeHash, code);

        if (!isValid)
        {
            entry.Attempts++;

            var json = JsonSerializer.Serialize(entry);

            var ttl = await _redis.KeyTimeToLiveAsync(key);
            var newTtl = (ttl.HasValue && ttl.Value > TimeSpan.Zero)
                ? ttl.Value
                : CodeTtl;

            await _redis.StringSetAsync(key, json, newTtl);

            return (false, "Invalid code.");
        }

        await _redis.KeyDeleteAsync(key);
        return (true, null);
    }

    public async Task RemoveCodeAsync(string email, string purpose = "default")
    {
        var normalizedPurpose = NormalizePurpose(purpose);
        var key = BuildKey(email, normalizedPurpose);

        await _redis.KeyDeleteAsync(key);
    }

    private static string NormalizePurpose(string purpose)
        => purpose.ToLowerInvariant();

    private static string BuildKey(string email, string purpose)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        var emailHash = Convert.ToHexString(hash);

        return $"{KeyPrefix}{purpose}:{emailHash}";
    }

    private string ComputeOtpHash(string code)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_otpPepper));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hashBytes);
    }

    private bool VerifyHash(string storedHash, string inputCode)
    {
        var computedHash = ComputeOtpHash(inputCode);

        var storedBytes = Convert.FromBase64String(storedHash);
        var computedBytes = Convert.FromBase64String(computedHash);

        return CryptographicOperations.FixedTimeEquals(storedBytes, computedBytes);
    }

    public async Task<(bool Success, string? Error)> VerifyCodeAsync(string email, string code,
                                                                     string expectedPurpose = "default", bool deleteAfterVerify = true)
    {
        var normalizedPurpose = NormalizePurpose(expectedPurpose);
        var key = BuildKey(email, normalizedPurpose);

        var value = await _redis.StringGetAsync(key);
        if (value.IsNull)
            return (false, "Code expired or not found.");

        OtpEntry? entry;
        try
        {
            entry = JsonSerializer.Deserialize<OtpEntry>(value!);
        }
        catch (JsonException)
        {
            await _redis.KeyDeleteAsync(key);
            return (false, "Invalid stored data.");
        }

        if (entry == null || string.IsNullOrEmpty(entry.CodeHash))
            return (false, "Invalid stored data.");

        if (entry.Purpose != normalizedPurpose)
            return (false, "Incorrect code assignment.");

        if (entry.Attempts >= MaxAttempts)
            return (false, "Too many attempts. Request a new code.");

        var isValid = VerifyHash(entry.CodeHash, code);

        if (!isValid)
        {
            entry.Attempts++;

            var json = JsonSerializer.Serialize(entry);
            var ttl = await _redis.KeyTimeToLiveAsync(key);
            var newTtl = (ttl.HasValue && ttl.Value > TimeSpan.Zero) ? ttl.Value : CodeTtl;

            await _redis.StringSetAsync(key, json, newTtl);

            return (false, "Invalid code.");
        }

        if (deleteAfterVerify)
        {
            await _redis.KeyDeleteAsync(key);
        }

        return (true, null);
    }
}