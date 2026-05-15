namespace Tailly.SpecialistService.Application.Dtos.Requests.Calendar;

public sealed class CreateAvailableSlotRequest
{
    public string Date { get; set; } = default!;
    public string StartTime { get; set; } = default!;
    public string EndTime { get; set; } = default!;
    public Guid? ServiceId { get; set; }
}