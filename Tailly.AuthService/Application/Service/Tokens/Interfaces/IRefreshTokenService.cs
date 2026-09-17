namespace Tailly.AuthService.Application.Service.Tokens.Interfaces;

public interface IRefreshTokenService
{
    (string rawToken, string hashedToken) GenerateToken();
    string HashToken(string rawToken);
    DateTime GetRefreshTokenExpiryDate();
}