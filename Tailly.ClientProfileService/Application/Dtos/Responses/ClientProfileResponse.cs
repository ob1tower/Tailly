namespace Tailly.ClientProfileService.Application.Dtos.Responses;

public sealed class ClientProfileResponse
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string Phone { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? CityId { get; set; }
    public string AvatarUrl { get; set; } = default!;
}