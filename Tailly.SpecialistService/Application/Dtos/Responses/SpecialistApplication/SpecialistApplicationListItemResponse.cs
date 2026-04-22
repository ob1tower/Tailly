namespace Tailly.SpecialistService.Application.Dtos.Responses.SpecialistApplication;

public sealed class SpecialistApplicationListItemResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}