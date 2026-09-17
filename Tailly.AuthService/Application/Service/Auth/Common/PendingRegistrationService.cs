using StackExchange.Redis;
using System.Text.Json;
using Tailly.AuthService.Core.Models.Internal;

namespace Tailly.AuthService.Application.Service.Auth.Common;

public class PendingRegistrationService : IPendingRegistrationService
{
    private readonly IDatabase _redis;

    private const string KeyPrefix = "register:";
    private const string TokenPrefix = "register-token:";
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

    public async Task AttachTokenAsync(string registrationId, string verificationToken)
    {
        await _redis.StringSetAsync(
            BuildTokenKey(verificationToken),
            registrationId,
            Ttl
        );
    }

    public async Task<(string Email, string PasswordHash)?> GetByTokenAsync(string verificationToken)
    {
        var registrationId = await _redis.StringGetAsync(BuildTokenKey(verificationToken));

        if (registrationId.IsNullOrEmpty)
            return null;

        return await GetAsync(registrationId!);
    }

    public async Task RemoveByTokenAsync(string verificationToken)
    {
        var registrationId = await _redis.StringGetAsync(BuildTokenKey(verificationToken));

        if (!registrationId.IsNullOrEmpty)
        {
            await RemoveAsync(registrationId!);
        }

        await _redis.KeyDeleteAsync(BuildTokenKey(verificationToken));
    }

    public async Task RemoveAsync(string registrationId)
    {
        await _redis.KeyDeleteAsync(BuildKey(registrationId));
    }

    private static string BuildKey(string id) => $"{KeyPrefix}{id}";
    private static string BuildTokenKey(string token) => $"{TokenPrefix}{token}";
}