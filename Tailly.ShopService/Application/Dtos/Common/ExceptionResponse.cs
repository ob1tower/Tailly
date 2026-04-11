namespace Tailly.ShopService.Application.Dtos.Common;

public sealed class ExceptionResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = default!;
}