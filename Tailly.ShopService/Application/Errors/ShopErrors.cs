using Tailly.ShopService.Core.Common;

namespace Tailly.ShopService.Application.Errors;

public static class ShopErrors
{
    public static readonly Error ProductNotFound =
        new("Product.NotFound", "Product not found.");

    public static readonly Error InvalidSlug =
        new("Product.InvalidSlug", "Product slug cannot be empty.");

    public static readonly Error InvalidProductIds =
        new("Product.InvalidIds", "Invalid product IDs format.");

    public static readonly Error InvalidPriceRange =
        new("Product.InvalidPriceRange", "Minimum price cannot be greater than maximum price.");

    public static readonly Error PickupPointNotFound =
        new("PickupPoint.NotFound", "Pickup point not found.");

    public static readonly Error InvalidCityName =
        new("PickupPoint.InvalidCity", "Invalid city name.");

    public static readonly Error OrderNotFound = 
        new("Order.NotFound", "Order not found.");

    public static readonly Error CartIsEmpty = 
        new("Order.CartIsEmpty", "Cannot create order: cart is empty.");

    public static readonly Error OrderCannotBeCancelled = 
        new("Order.CannotBeCancelled", "This order cannot be cancelled anymore.");

    public static readonly Error InvalidDeliveryMethod = 
        new("Order.InvalidDeliveryMethod", "Invalid delivery method.");

    public static readonly Error InvalidPaymentMethod = 
        new("Order.InvalidPaymentMethod", "Invalid payment method.");

    public static readonly Error PickupPointRequired = 
        new("Order.PickupPointRequired", "Pickup point is required for pickup delivery.");

    public static readonly Error AddressRequired = 
        new("Order.AddressRequired", "Delivery address is required for courier delivery.");

    public static readonly Error InvalidForm = 
        new("Order.InvalidForm", "Checkout form cannot be null.");

    public static readonly Error InvalidRecipient = 
        new("Order.InvalidRecipient", "Recipient information is required.");

    public static readonly Error OrderAlreadyPaid = 
        new("Order.AlreadyPaid", "This order is already paid.");

    public static readonly Error OrderAlreadyCancelled =
        new("Order.AlreadyCancelled", "This order has already been cancelled.");

    public static readonly Error PaymentFailed = 
        new("Payment.Failed", "Payment processing failed.");

    public static readonly Error InvalidQuantity =
        new("Cart.InvalidQuantity", "Quantity must be greater than zero.");

    public static readonly Error ItemAlreadyExists =
        new("Cart.ItemAlreadyExists", "Item already exists in cart.");

    public static readonly Error ItemNotFound =
        new("Cart.ItemNotFound", "Item not found in cart.");

    public static readonly Error CartIdentityRequired = 
        new("Cart.IdentityRequired", "UserId or SessionId must be provided.");

    public static readonly Error FavoriteIdentityRequired =
        new("FavoriteIdentityRequired", "UserId or SessionId required.");

    public static readonly Error FavoriteAlreadyExists =
        new("FavoriteAlreadyExists", "Item already in favorites.");

    public static readonly Error FavoriteNotFound =
        new("FavoriteNotFound", "Item not found in favorites.");

    public static readonly Error ReviewNotFound =
        new("Review.NotFound", "Review not found.");

    public static readonly Error ReviewAlreadyHasReply =
        new("Review.AlreadyHasReply", "This review already has a reply from the shop.");

    public static readonly Error OrderNotCompleted =
        new("Review.OrderNotCompleted", "You can only leave a review for delivered orders.");

    public static readonly Error ProductNotInOrder =
        new("Review.ProductNotInOrder", "This product was not purchased in the specified order.");

    public static readonly Error ReviewAlreadyExists =
        new("Review.AlreadyExists", "You have already reviewed this product in this order.");
}