namespace Tailly.AuthService.Application.Dtos.Requests.EmailChange;

public sealed class RequestEmailChangeRequest
{
    public string NewEmail { get; set; } = default!;
}