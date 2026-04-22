using Tailly.SpecialistService.Application.Dtos.Responses.Common;

namespace Tailly.SpecialistService.Application.Dtos.Responses.Short;

public sealed class SpecialistShortResponse
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public GeoPointResponse Location { get; set; } = default!;
    public List<ServiceShortResponse> Services { get; set; } = [];
}