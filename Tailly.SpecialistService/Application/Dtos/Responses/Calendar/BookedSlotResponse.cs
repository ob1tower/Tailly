namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public class BookedSlotResponse
{
    public Guid Id { get; set; }
    public string Date { get; set; } = default!;
    public string StartTime { get; set; } = default!;
    public string EndTime { get; set; } = default!;
    public Guid? OrderId { get; set; }
    public Guid? ServiceId { get; set; }
}