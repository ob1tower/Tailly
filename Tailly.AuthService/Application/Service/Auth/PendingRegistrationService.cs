using StackExchange.Redis;
using System.Text.Json;
using Tailly.AuthService.Application.Service.Auth.Interfaces;
using Tailly.AuthService.Core.Models.Internal;

namespace Tailly.AuthService.Application.Service.Auth;

public class PendingRegistrationService : IPendingRegistrationService
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "register:";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);

    public PendingRegistrationService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task<string> CreateAsync(string email, string passwordHash)
    {
        var registrationId = Guid.NewGuid().ToString();

        var data = new RegistrationData
        {
            Email = email,
            PasswordHash = passwordHash
        };

        var json = JsonSerializer.Serialize(data);

        await _redis.StringSetAsync(
            BuildKey(registrationId),
            json,
            Ttl);

        return registrationId;
    }

    public async Task<(string Email, string PasswordHash)?> GetAsync(string registrationId)
    {
        var value = await _redis.StringGetAsync(BuildKey(registrationId));

        if (value.IsNullOrEmpty)
            return null;

        var data = JsonSerializer.Deserialize<RegistrationData>(value!);

        return (data!.Email, data.PasswordHash);
    }

    public async Task RemoveAsync(string registrationId)
    {
        await _redis.KeyDeleteAsync(BuildKey(registrationId));
    }

    private static string BuildKey(string id) => $"{KeyPrefix}{id}";
}