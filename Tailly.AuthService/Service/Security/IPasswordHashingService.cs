namespace Tailly.AuthService.Service.Security
{
    public interface IPasswordHashingService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}