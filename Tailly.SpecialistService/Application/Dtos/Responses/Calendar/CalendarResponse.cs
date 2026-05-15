namespace Tailly.SpecialistService.Application.Dtos.Responses.Calendar;

public sealed class CalendarResponse
{
    public Guid Id { get; set; }
    public List<AvailabilityWindowResponse> AvailabilityWindows { get; set; } = [];
    public List<DayOverrideResponse> DayOverrides { get; set; } = [];
    public List<BookedSlotResponse> BookedSlots { get; set; } = [];
}