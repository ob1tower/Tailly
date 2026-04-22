namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public sealed class SpecialistCalendarAvailabilityWindowResponse
{
    public string Id { get; set; } = default!;
    public string Date { get; set; } = default!;
    public string StartTime { get; set; } = default!;
    public string EndTime { get; set; } = default!;
    public List<string> ServiceIds { get; set; } = [];
}