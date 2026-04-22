namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public sealed class SpecialistCalendarResponse
{
    public string Timezone { get; set; } = "Europe/Moscow";
    public List<SpecialistCalendarDayOverrideResponse> DayOverrides { get; set; } = [];
    public List<SpecialistCalendarBookedSlotResponse> BookedSlots { get; set; } = [];
    public List<SpecialistCalendarAvailabilityWindowResponse> AvailabilityWindows { get; set; } = [];
    public SpecialistCalendarBookingSettingsResponse? BookingSettings { get; set; }
    public List<object> AvailabilityRules { get; set; } = [];
    public List<object> AvailabilityOverrides { get; set; } = [];
}