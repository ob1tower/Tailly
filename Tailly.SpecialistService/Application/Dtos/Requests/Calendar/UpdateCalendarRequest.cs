namespace Tailly.SpecialistService.Application.Dtos.Requests.Calendar;

public sealed class UpdateCalendarRequest
{
    public List<CreateAvailableSlotRequest> AvailabilityWindows { get; set; } = [];
    public List<CreateDayOverrideRequest> DayOverrides { get; set; } = [];
}