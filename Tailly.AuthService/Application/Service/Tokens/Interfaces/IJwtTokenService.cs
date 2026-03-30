using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Application.Service.Tokens.Interfaces;

public interface IJwtTokenService
{
    Task<(string token, DateTime expires)> CreateAccessTokenAsync(UserEntity user);
}