namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public class AvailabilityWindowResponse
{
    public Guid Id { get; set; }
    public string Date { get; set; } = default!;
    public string StartTime { get; set; } = default!;
    public string EndTime { get; set; } = default!;
    public Guid? ServiceId { get; set; }
}