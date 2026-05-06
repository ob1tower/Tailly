namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public sealed class CalendarResponse
{
    public string Timezone { get; set; } = "Europe/Moscow";
    public List<AvailabilityWindowResponse> AvailabilityWindows { get; set; } = [];
    public List<ManualOverrideResponse> DayOverrides { get; set; } = [];
    public List<BookedSlotResponse> BookedSlots { get; set; } = [];
}