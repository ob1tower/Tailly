namespace Tailly.AuthService.Application.Service.Auth.Interfaces;

public interface IPendingRegistrationService
{
    Task<string> CreateAsync(string email, string passwordHash);
    Task<(string Email, string PasswordHash)?> GetAsync(string registrationId);
    Task RemoveAsync(string registrationId);
}