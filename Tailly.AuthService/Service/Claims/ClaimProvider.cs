using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tailly.AuthService.Entities;
using Tailly.AuthService.Enums;

namespace Tailly.AuthService.Service.Claims;

public class ClaimProvider
{
    public Task<List<Claim>> GenerateClaimsAsync(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };
        claims.AddRange(
            user.UserRoles.Select(r =>
                new Claim(ClaimTypes.Role, ((RoleType)r.RoleId).ToString()))
        );

        return Task.FromResult(claims);
    }
}
