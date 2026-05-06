namespace Tailly.BookingService.Core.Common;

public static class OrderNumberGenerator
{
    public static string Generate()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = Random.Shared.Next(10000, 99999);

        return $"SO-{date}-{random}";
    }
}