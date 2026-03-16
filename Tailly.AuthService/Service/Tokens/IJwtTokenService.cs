using Tailly.AuthService.Entities;

namespace Tailly.AuthService.Service.Tokens
{
    public interface IJwtTokenService
    {
        Task<(string token, DateTime expires)> CreateAccessTokenAsync(UserEntity user);
    }
}