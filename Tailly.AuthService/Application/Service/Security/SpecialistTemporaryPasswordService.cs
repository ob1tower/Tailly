using StackExchange.Redis;

namespace Tailly.AuthService.Application.Service.Security;

public class SpecialistTemporaryPasswordService
{
    private readonly IDatabase _redis;

    private const string KeyPrefix = "specialist:temp-password:";
    private static readonly TimeSpan PasswordTtl = TimeSpan.FromMinutes(15);

    public SpecialistTemporaryPasswordService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task SaveAsync(Guid specialistId, string password)
    {
        var key = BuildKey(specialistId);

        await _redis.StringSetAsync(
            key,
            password,
            PasswordTtl);
    }

    public async Task<string?> GetAsync(Guid specialistId)
    {
        var key = BuildKey(specialistId);

        var value = await _redis.StringGetAsync(key);

        return value.IsNullOrEmpty
            ? null
            : value.ToString();
    }

    public async Task RemoveAsync(Guid specialistId)
    {
        var key = BuildKey(specialistId);

        await _redis.KeyDeleteAsync(key);
    }

    private static string BuildKey(Guid specialistId)
    {
        return $"{KeyPrefix}{specialistId}";
    }
}