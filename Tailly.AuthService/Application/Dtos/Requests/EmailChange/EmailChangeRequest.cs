namespace Tailly.AuthService.Application.Dtos.Requests.EmailChange;

public sealed class EmailChangeRequest
{
    public string NewEmail { get; set; } = default!;
}