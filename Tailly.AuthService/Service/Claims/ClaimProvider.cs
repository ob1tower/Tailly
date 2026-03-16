using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tailly.AuthService.Entities;

namespace Tailly.AuthService.Service.Claims;

public class ClaimProvider
{
    public Task<List<Claim>> GenerateClaimsAsync(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.RoleId.ToString())
        };

        return Task.FromResult(claims);
    }
}
