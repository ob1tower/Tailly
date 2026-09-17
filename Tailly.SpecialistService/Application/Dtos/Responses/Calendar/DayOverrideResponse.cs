namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public sealed class DayOverrideResponse
{
    public Guid Id { get; set; }
    public string Date { get; set; } = default!;
    public string Status { get; set; } = default!;
}