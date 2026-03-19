using StackExchange.Redis;
using System.Text.Json;

namespace Tailly.AuthService.Service.Security;

public class VerificationCodeService : IVerificationCodeService
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "verify:email:";

    public VerificationCodeService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task SetCodeAsync(string email, string code)
    {
        var key = $"{KeyPrefix}{email}";
        var value = JsonSerializer.Serialize(new
        {
            code,
            createdAt = DateTime.UtcNow
        });

        await _redis.StringSetAsync(key, value, TimeSpan.FromMinutes(15));
    }

    public async Task<string?> GetCodeAsync(string email)
    {
        var key = $"{KeyPrefix}{email}";
        var value = await _redis.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        var obj = JsonSerializer.Deserialize<JsonElement>(value!);

        return obj.GetProperty("code").GetString();
    }

    public async Task RemoveCodeAsync(string email)
    {
        var key = $"{KeyPrefix}{email}";
        await _redis.KeyDeleteAsync(key);
    }

    public async Task<bool> VerifyCodeAsync(string email, string code)
    {
        var storedCode = await GetCodeAsync(email);

        if (storedCode == null)
            return false;

        if (storedCode != code)
            return false;

        await RemoveCodeAsync(email);

        return true;
    }
}