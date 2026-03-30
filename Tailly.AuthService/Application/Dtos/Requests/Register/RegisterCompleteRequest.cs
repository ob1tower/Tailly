namespace Tailly.AuthService.Application.Dtos.Requests.Register;

public sealed class RegisterCompleteRequest
{
    public string RegistrationId { get; set; } = default!;
    public string VerificationToken { get; set; } = default!;
}