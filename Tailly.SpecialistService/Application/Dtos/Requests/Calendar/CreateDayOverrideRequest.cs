namespace Tailly.SpecialistService.Application.Dtos.Requests.Calendar;

public sealed class CreateDayOverrideRequest
{
    public string Date { get; set; } = default!;
    public string Status { get; set; } = default!;
}