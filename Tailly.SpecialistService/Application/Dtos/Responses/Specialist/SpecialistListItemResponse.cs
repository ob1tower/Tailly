using Tailly.SpecialistService.Application.Dtos.Responses.Services;

namespace Tailly.SpecialistService.Application.Dtos.Responses.Specialist;

public sealed class SpecialistListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public int ExperienceYears { get; set; }
    public LocationResponse Location { get; set; } = new();
    public List<ServiceShortResponse> Services { get; set; } = [];
}