using StackExchange.Redis;

namespace Tailly.SpecialistService.Application.Service.Security;

public class SpecialistTemporaryPasswordService
{
    private readonly IDatabase _redis;

    private const string KeyPrefix = "specialist:temp-password:";

    public SpecialistTemporaryPasswordService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
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