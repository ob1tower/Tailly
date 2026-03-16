namespace Tailly.AuthService.Service.Tokens
{
    public interface IRefreshTokenService
    {
        (string rawToken, string hashedToken) GenerateToken();
        string HashToken(string rawToken);
    }
}