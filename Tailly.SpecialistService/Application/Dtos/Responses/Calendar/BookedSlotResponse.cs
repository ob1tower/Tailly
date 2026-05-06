namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public class BookedSlotResponse
{
    public string Date { get; set; } = default!;
    public string StartTime { get; set; } = default!;
    public string EndTime { get; set; } = default!;
}