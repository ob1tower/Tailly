namespace Tailly.AuthService.Application.Service.Auth.Interfaces
{
    public interface IPendingRegistrationService
    {
        Task AttachTokenAsync(string registrationId, string verificationToken);
        Task<string> CreateAsync(string email, string passwordHash);
        Task<(string Email, string PasswordHash)?> GetAsync(string registrationId);
        Task<(string Email, string PasswordHash)?> GetByTokenAsync(string verificationToken);
        Task RemoveAsync(string registrationId);
        Task RemoveByTokenAsync(string verificationToken);
    }
}