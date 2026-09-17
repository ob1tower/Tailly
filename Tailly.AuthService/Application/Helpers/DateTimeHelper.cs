namespace Tailly.AuthService.Application.Helpers;

public static class DateTimeHelper
{
    public static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime(),
        _ => value.ToUniversalTime()
    };
}