namespace Tailly.SpecialistService.Application.Dtos.Responses.Specialist;

public sealed class SpecialistMainResponse
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string MiddleName { get; set; } = default!;
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;
}