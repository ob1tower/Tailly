using Tailly.SpecialistService.Core.Entities.Specialist;

namespace Tailly.SpecialistService.Core.Entities.Calendar;

public class CalendarEntity
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public SpecialistEntity Specialist { get; set; } = null!;
    public string Timezone { get; set; } = "Europe/Moscow";
    public ICollection<CalendarDayOverrideEntity> DayOverrides { get; set; } = [];
    public ICollection<CalendarBookedSlotEntity> BookedSlots { get; set; } = [];
    public ICollection<CalendarAvailabilityWindowEntity> AvailabilityWindows { get; set; } = [];
    public CalendarBookingSettingsEntity? BookingSettings { get; set; }
}