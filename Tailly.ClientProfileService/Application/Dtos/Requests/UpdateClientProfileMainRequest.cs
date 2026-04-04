namespace Tailly.ClientProfileService.Application.Dtos.Requests;

public sealed class UpdateClientProfileMainRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string? AvatarUrl { get; set; }
}