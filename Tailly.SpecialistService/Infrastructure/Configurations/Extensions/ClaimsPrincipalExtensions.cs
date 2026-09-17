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

    public static Guid? GetSpecialistId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("specialistId")?.Value;

        return Guid.TryParse(claim, out var id)
            ? id
            : null;
    }
}