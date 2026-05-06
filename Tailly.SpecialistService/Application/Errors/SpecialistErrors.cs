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
}
