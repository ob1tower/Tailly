namespace Tailly.AuthService.Application.Dtos.Requests.Admin;

public sealed class ConfirmEmailChangePayloadRequest
{
    public string Code { get; set; } = default!;
}