namespace Tailly.SpecialistService.Core.Entities.Calendar;

public class CalendarBookingSettingsEntity
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public CalendarEntity Calendar { get; set; } = null!;
    public string DayStartTime { get; set; } = "08:00";
    public string DayEndTime { get; set; } = "22:00";
    public int SlotStepMinutes { get; set; } = 30;
    public int DefaultDurationMinutes { get; set; } = 60;
}