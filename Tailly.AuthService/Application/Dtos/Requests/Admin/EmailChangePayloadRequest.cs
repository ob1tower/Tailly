namespace Tailly.AuthService.Application.Dtos.Requests.Admin;

public sealed class EmailChangePayloadRequest
{
    public string NewEmail { get; set; } = default!;
    public string Password { get; set; } = default!;
}