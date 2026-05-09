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

    public static readonly Error CannotCancelActiveOrder =
        new("Order.CannotCancelActive", "An active order cannot be cancelled.");

    public static readonly Error InvalidStatusTransition =
        new("Order.InvalidStatus", "The order status cannot be changed.");

    public static readonly Error InvalidOrderStatus =
        new("Order.InvalidOrderStatus", "Invalid order status for this operation.");

    public static readonly Error OrderExpired =
        new("Order.Expired", "The order has already expired.");

    public static readonly Error TooEarlyToStartOrder =
        new("Order.TooEarlyToStart", "It is too early to start the order.");

    public static readonly Error OrderNotStarted =
        new("Order.NotStarted", "The order has not been started yet.");

    public static readonly Error OrderNotCompleted =
        new("Order.NotCompleted", "The order has not been completed.");

    public static readonly Error InvalidOrderDate =
        new("Order.InvalidDate", "The order date is invalid.");

    public static readonly Error InvalidOrderDateRange =
        new("Order.InvalidDateRange", "The order end date must be later than the start date.");

    public static readonly Error CannotRepeatNotCompletedOrder =
        new("Order.CannotRepeat", "Only completed orders can be repeated.");

    public static readonly Error AlreadyReviewed =
        new("Review.AlreadyExists", "A review has already been left for this order.");

    public static readonly Error ReviewNotFound =
        new("Review.NotFound", "Review not found.");

    public static readonly Error CannotReviewNotCompletedOrder =
        new("Review.CannotReview", "You can't leave a review for an incomplete order.");

    public static readonly Error ReviewAlreadyExists =
        new("Review.ReviewAlreadyExists", "Review has already been left for this order.");

    public static readonly Error InvalidClient =
        new("Order.InvalidClient", "Invalid client.");

    public static readonly Error InvalidSpecialist =
        new("Order.InvalidSpecialist", "Invalid specialist.");

    public static readonly Error InvalidService =
        new("Order.InvalidService", "Invalid service.");

    public static readonly Error InvalidPet =
        new("Order.InvalidPet", "Invalid pet.");

    public static readonly Error InvalidPrice =
        new("Order.InvalidPrice", "Invalid price.");
}