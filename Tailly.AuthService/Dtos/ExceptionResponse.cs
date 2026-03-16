namespace Tailly.AuthService.Dtos;

public sealed class ExceptionResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = default!;
}
