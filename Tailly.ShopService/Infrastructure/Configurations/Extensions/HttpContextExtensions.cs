using Tailly.ShopService.Infrastructure.Configurations.Constants;

namespace Tailly.ShopService.Infrastructure.Configurations.Extensions;

public static class HttpContextExtensions
{
    public static Guid GetOrCreateSessionId(this HttpContext context)
    {
        var session = context.Request.Cookies[ConnectionStrings.SessionCookieName];

        if (Guid.TryParse(session, out var sessionId))
            return sessionId;

        var newSessionId = Guid.NewGuid();

        context.Response.Cookies.Append(
            ConnectionStrings.SessionCookieName,
            newSessionId.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

        return newSessionId;
    }

    public static Guid? GetSessionId(this HttpContext context)
    {
        var session = context.Request.Cookies["sessionId"];

        if (Guid.TryParse(session, out var sessionId))
            return sessionId;

        return null;
    }
}