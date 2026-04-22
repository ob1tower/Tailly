using System.Security.Claims;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Extensions;

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