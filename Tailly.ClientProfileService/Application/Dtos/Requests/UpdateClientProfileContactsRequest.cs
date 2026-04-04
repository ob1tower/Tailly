namespace Tailly.ClientProfileService.Application.Dtos.Requests;

public sealed class UpdateClientProfileContactsRequest
{
    public string Phone { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? CityId { get; set; }
}