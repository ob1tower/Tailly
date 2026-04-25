using System.Security.Claims;
using Tailly.AuthService.Core.Enums;

namespace Tailly.AuthService.Infrastructure.Configurations.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userId, out var parsedUserId))
            return parsedUserId;

        return null;
    }
}