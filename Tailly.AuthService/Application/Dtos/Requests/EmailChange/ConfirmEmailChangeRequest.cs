namespace Tailly.AuthService.Application.Dtos.Requests.EmailChange;

public sealed class ConfirmEmailChangeRequest
{
    public string RequestId { get; set; } = default!;
    public string NewEmail { get; set; } = default!;
    public string Code { get; set; } = default!;
}