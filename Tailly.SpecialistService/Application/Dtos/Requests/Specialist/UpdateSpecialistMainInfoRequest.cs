namespace Tailly.SpecialistService.Application.Dtos.Requests.Specialist;

public sealed class UpdateSpecialistMainInfoRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? AvatarUrl { get; set; }
}