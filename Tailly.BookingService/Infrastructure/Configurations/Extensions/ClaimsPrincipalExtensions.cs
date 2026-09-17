using System.Security.Claims;

namespace Tailly.BookingService.Infrastructure.Configurations.Extensions;

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
        var claim = user.FindFirst("specialistId");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
