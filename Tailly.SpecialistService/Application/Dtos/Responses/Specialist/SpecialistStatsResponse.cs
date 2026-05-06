namespace Tailly.SpecialistService.Application.Dtos.Responses.Specialist;

public sealed class SpecialistStatsResponse
{
    public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public int CompletedOrdersCount { get; set; }
    public int RepeatOrdersCount { get; set; }
}