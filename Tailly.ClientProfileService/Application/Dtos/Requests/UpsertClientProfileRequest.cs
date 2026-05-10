namespace Tailly.ClientProfileService.Application.Dtos.Requests;

public sealed class UpsertClientProfileRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string Phone { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? CityId { get; set; }
    public string? District { get; set; }
    public string? AvatarUrl { get; set; }
}