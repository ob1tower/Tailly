namespace Tailly.ShopService.Core.Common;

public static class OrderNumberGenerator
{
    public static string Generate()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = Random.Shared.Next(100000, 999999);

        return $"ORD-{date}-{random}";
    }
}