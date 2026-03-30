using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tailly.AuthService.Application.Service.Claims;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Infrastructure.Configurations.Options;

namespace Tailly.AuthService.Application.Service.Tokens;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly ClaimProvider _claimProvider;

    public JwtTokenService(IOptions<JwtOptions> options,
                           ClaimProvider claimProvider)
    {
        _options = options.Value;
        _claimProvider = claimProvider;
    }

    public async Task<(string token, DateTime expires)> CreateAccessTokenAsync(UserEntity user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = await _claimProvider.GenerateClaimsAsync(user);

        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        claims.Add(new Claim(
            JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64));

        var expires = DateTime.UtcNow.AddMinutes(_options.AccessExpiresMinutes);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return (token, expires);
    }
}