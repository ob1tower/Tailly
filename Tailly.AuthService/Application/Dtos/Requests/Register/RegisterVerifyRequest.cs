namespace Tailly.AuthService.Application.Dtos.Requests.Register;

public sealed class RegisterVerifyRequest
{
    public string RegistrationId { get; set; } = default!;
    public string Code { get; set; } = default!;
}