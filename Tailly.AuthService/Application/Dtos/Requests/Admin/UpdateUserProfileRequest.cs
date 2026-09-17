namespace Tailly.AuthService.Application.Dtos.Requests.Admin;

public sealed class UpdateUserProfileRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string? SpecialistSlug { get; set; }
}