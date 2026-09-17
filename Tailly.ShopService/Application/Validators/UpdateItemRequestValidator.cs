using FluentValidation;
using Tailly.ShopService.Application.Dtos.Requests.Cart;

namespace Tailly.ShopService.Application.Validators;

public class UpdateItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    private const int MIN_QUANTITY = 1;
    private const int MAX_QUANTITY = 1000;

    public UpdateItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(MIN_QUANTITY)
            .WithMessage($"Quantity must be at least {MIN_QUANTITY}.")
            .LessThanOrEqualTo(MAX_QUANTITY)
            .WithMessage($"Quantity must not exceed {MAX_QUANTITY}.");
    }
}