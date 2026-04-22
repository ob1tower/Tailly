namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public sealed class SpecialistCalendarDayOverrideResponse
{
    public string Date { get; set; } = default!;
    public string Status { get; set; } = "available";  
}