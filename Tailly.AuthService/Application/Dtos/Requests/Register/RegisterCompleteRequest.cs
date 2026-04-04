namespace Tailly.AuthService.Application.Dtos.Requests.Register;

public sealed class RegisterCompleteRequest
{
    public string VerificationToken { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string CityId { get; set; } = default!;
    public string? CityName { get; set; }
}