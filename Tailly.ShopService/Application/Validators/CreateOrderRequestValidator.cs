using FluentValidation;
using Tailly.ShopService.Application.Dtos.Requests.Order;

namespace Tailly.ShopService.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    private const int MAX_NAME_LENGTH = 100;
    private const int MAX_PHONE_LENGTH = 30;
    private const int MAX_EMAIL_LENGTH = 256;
    private const int MAX_ADDRESS_FIELD_LENGTH = 200;
    private const int MAX_HOUSE_LENGTH = 50;
    private const int MAX_APARTMENT_LENGTH = 50;
    private const int MAX_COMMENT_LENGTH = 500;

    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Form)
            .NotNull().WithMessage("Checkout form is required.");

        When(x => x.Form != null, () =>
        {
            RuleFor(x => x.Form.Recipient)
                .NotNull().WithMessage("Recipient information is required.");

            RuleFor(x => x.Form.DeliveryMethod)
                .NotEmpty().WithMessage("Delivery method is required.")
                .Must(method => method?.ToLowerInvariant() is "courier" or "pickup-point" or "pickup")
                .WithMessage("Invalid delivery method. Allowed values: courier, pickup-point, pickup.");

            RuleFor(x => x.Form.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required.")
                .Must(method => method?.ToLowerInvariant() is "card" or "sbp" or "cash" or "card-on-delivery")
                .WithMessage("Invalid payment method. Allowed values: card, sbp, cash, card-on-delivery.");

            When(x => x.Form.DeliveryMethod?.ToLowerInvariant() == "courier", () =>
            {
                RuleFor(x => x.Form.Address)
                    .NotNull().WithMessage("Delivery address is required for courier delivery.");
            });

            When(x => x.Form.DeliveryMethod?.ToLowerInvariant() is "pickup-point" or "pickup", () =>
            {
                RuleFor(x => x.Form.PickupPointId)
                    .NotEmpty().WithMessage("Pickup point ID is required for pickup delivery.");
            });

            When(x => x.Form.Recipient != null, () =>
            {
                RuleFor(x => x.Form.Recipient.FirstName)
                    .NotEmpty().WithMessage("First name is required.")
                    .MaximumLength(MAX_NAME_LENGTH).WithMessage($"First name must not exceed {MAX_NAME_LENGTH} characters.");

                RuleFor(x => x.Form.Recipient.LastName)
                    .NotEmpty().WithMessage("Last name is required.")
                    .MaximumLength(MAX_NAME_LENGTH).WithMessage($"Last name must not exceed {MAX_NAME_LENGTH} characters.");

                RuleFor(x => x.Form.Recipient.Phone)
                    .NotEmpty().WithMessage("Phone is required.")
                    .MaximumLength(MAX_PHONE_LENGTH).WithMessage($"Phone must not exceed {MAX_PHONE_LENGTH} characters.");

                RuleFor(x => x.Form.Recipient.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .MaximumLength(MAX_EMAIL_LENGTH).WithMessage($"Email must not exceed {MAX_EMAIL_LENGTH} characters.")
                    .EmailAddress().WithMessage("Invalid email format.");
            });

            When(x => x.Form?.Address != null, () =>
            {
                RuleFor(x => x.Form!.Address!.City)
                    .NotEmpty().WithMessage("City is required.")
                    .MaximumLength(MAX_ADDRESS_FIELD_LENGTH).WithMessage($"City must not exceed {MAX_ADDRESS_FIELD_LENGTH} characters.");

                RuleFor(x => x.Form!.Address!.Street)
                    .NotEmpty().WithMessage("Street is required.")
                    .MaximumLength(MAX_ADDRESS_FIELD_LENGTH).WithMessage($"Street must not exceed {MAX_ADDRESS_FIELD_LENGTH} characters.");

                RuleFor(x => x.Form!.Address!.House)
                    .NotEmpty().WithMessage("House is required.")
                    .MaximumLength(MAX_HOUSE_LENGTH).WithMessage($"House must not exceed {MAX_HOUSE_LENGTH} characters.");

                RuleFor(x => x.Form!.Address!.Apartment)
                    .MaximumLength(MAX_APARTMENT_LENGTH).WithMessage($"Apartment must not exceed {MAX_APARTMENT_LENGTH} characters.");

                RuleFor(x => x.Form!.Address!.Comment)
                    .MaximumLength(MAX_COMMENT_LENGTH).WithMessage($"Comment must not exceed {MAX_COMMENT_LENGTH} characters.");
            });
        });
    }
}