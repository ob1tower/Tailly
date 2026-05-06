using Tailly.BookingService.Core.Common;

namespace Tailly.BookingService.Application.Errors;

public static class BookingErrors
{
    public static readonly Error OrderNotFound =
        new("Order.NotFound", "The order was not found.");

    public static readonly Error Forbidden =
        new("Order.Forbidden", "There is no access to this order.");

    public static readonly Error InvalidOrder =
        new("Order.Invalid", "Incorrect order data.");

    public static readonly Error SpecialistNotFound =
        new("Specialist.NotFound", "Specialist not found.");

    public static readonly Error PetNotFound =
        new("Pet.NotFound", "The pet was not found.");

    public static readonly Error ServiceNotFound =
        new("Service.NotFound", "The service was not found.");

    public static readonly Error InvalidSchedule =
        new("Order.InvalidSchedule", "Incorrect service schedule.");

    public static readonly Error CannotCancelOrder =
        new("Order.CannotCancel", "This order can no longer be cancelled.");

    public static readonly Error InvalidStatusTransition =
        new("Order.InvalidStatus", "The order status cannot be changed.");

    public static readonly Error AlreadyReviewed =
        new("Review.AlreadyExists", "A review has already been left for this order.");

    public static readonly Error ReviewNotFound =
        new("Review.NotFound", "Review not found.");

    public static readonly Error CannotReviewNotCompletedOrder =
        new("Review.CannotReview", "You can't leave a review for an incomplete order.");
}