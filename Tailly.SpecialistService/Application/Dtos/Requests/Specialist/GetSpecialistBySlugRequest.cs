namespace Tailly.SpecialistService.Application.Dtos.Requests.Specialist;

public sealed class GetSpecialistBySlugRequest
{
    public string Slug { get; set; } = default!;
}