namespace Tailly.AuthService.Application.Dtos.Responses.Register;

public sealed class RegisterVerifyResponse
{
    public string VerificationToken { get; set; } = default!;
}