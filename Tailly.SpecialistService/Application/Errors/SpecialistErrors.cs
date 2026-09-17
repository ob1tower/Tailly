using Tailly.SpecialistService.Core.Common;

namespace Tailly.SpecialistService.Application.Errors;

public static class SpecialistErrors
{
    public static readonly Error SpecialistNotFound =
        new("Specialist.NotFound", "Specialist not found.");

    public static readonly Error ServiceNotFound =
        new("Service.NotFound", "Service not found.");

    public static readonly Error ServiceDoesNotBelongToSpecialist =
        new("Service.DoesNotBelongToSpecialist", "Service does not belong to this specialist.");

    public static readonly Error ReviewNotFound =
        new("Review.NotFound", "Review not found.");

    public static readonly Error ReviewDoesNotBelongToSpecialist =
        new("Review.DoesNotBelongToSpecialist", "Review does not belong to this specialist.");

    public static readonly Error NotFound =
        new("Specialist.NotFound", "Specialist not found.");

    public static readonly Error InvalidSlug =
        new("Specialist.InvalidSlug", "Invalid specialist slug.");

    public static readonly Error InvalidPriceRange =
        new("Specialist.InvalidPriceRange", "Invalid price range.");

    public static readonly Error InvalidServiceId =
        new("Specialist.InvalidServiceId", "Invalid service id.");

    public static readonly Error Forbidden =
        new("Specialist.Forbidden", "No access.");

    public static readonly Error InvalidAvatar =
        new("Specialist.InvalidAvatar", "Incorrect avatar.");

    public static readonly Error ServiceTypeAlreadyExists =
        new("Service.TypeAlreadyExists", "This type of service already exists with a specialist.");

    public static readonly Error CalendarNotFound =
        new("Calendar.NotFound", "Calendar not found.");

    public static readonly Error AvailabilityWindowNotFound =
        new("Calendar.AvailabilityWindowNotFound", "Availability window not found.");

    public static readonly Error DayOverrideNotFound =
        new("Calendar.DayOverrideNotFound", "Day override not found.");

    public static readonly Error InvalidTimeRange =
        new("Calendar.InvalidTimeRange", "Start time must be less than end time.");

    public static readonly Error InvalidSlotStep =
        new("Calendar.InvalidSlotStep", "Slot step must be greater than zero.");

    public static readonly Error InvalidDefaultDuration =
        new("Calendar.InvalidDefaultDuration", "Default duration must be greater than zero.");

    public static readonly Error AvailabilityWindowIntersection =
        new("Calendar.AvailabilityWindowIntersection", "Availability window intersects with another window.");

    public static readonly Error DayOverrideAlreadyExists =
        new("Calendar.DayOverrideAlreadyExists", "Day override already exists for this date.");

    public static readonly Error InvalidSlotDuration =
        new("Calendar.InvalidSlotDuration", "Default duration must be divisible by slot step.");

    public static readonly Error AvailabilityWindowOutsideWorkingHours =
        new("Calendar.AvailabilityWindowOutsideWorkingHours", "Availability window is outside working hours.");

    public static readonly Error CalendarAlreadyExists =
        new("Calendar.AlreadyExists", "Calendar already exists.");

    public static readonly Error BookedSlotNotFound =
        new ("Calendar.BookedSlotNotFound", "Booked slot not found.");

    public static readonly Error InvalidDate = 
        new("Calendar.InvalidDate", "The date is invalid.");

    public static readonly Error BookedSlotIntersection = 
        new("Calendar.BookedSlotIntersection", "The booked slot intersects with another slot.");

    public static readonly Error DayOverrideConflict =
        new("Calendar.DayOverrideConflict", "Cannot create availability window for blocked day.");

    public static readonly Error CannotClearBookedDay = 
        new("Calendar.CannotClearBookedDay", "Cannot clear day with active bookings.");

    public static readonly Error CalendarDayIsEmpty = 
        new("Calendar.CalendarDayIsEmpty", "Calendar day is empty.");
}