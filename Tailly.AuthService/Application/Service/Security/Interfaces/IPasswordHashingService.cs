namespace Tailly.AuthService.Application.Service.Security.Interfaces;

public interface IPasswordHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}