namespace Tailly.ShopService.Infrastructure.Configurations.Extensions;

public static class IdentityExtensions
{
    public static (Guid? userId, Guid? sessionId) ResolveIdentity(this HttpContext context)
    {
        var userId = context.User.GetUserId();

        if (userId != null)
            return (userId, null);

        var sessionId = context.GetOrCreateSessionId();

        return (null, sessionId);
    }
}