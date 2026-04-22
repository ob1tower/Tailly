namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public sealed class SpecialistCalendarBookingSettingsResponse
{
    public string DayStartTime { get; set; } = "09:00";
    public string DayEndTime { get; set; } = "18:00";
    public int SlotStepMinutes { get; set; } = 30;
    public int DefaultDurationMinutes { get; set; } = 60;
}