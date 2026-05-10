using FluentValidation;
using Tailly.ShopService.Application.Dtos.Requests.Product;

namespace Tailly.ShopService.Application.Validators;

public class CreateProductReviewRequestValidator : AbstractValidator<CreateProductReviewRequest>
{
    private const int MIN_RATING = 1;
    private const int MAX_RATING = 5;

    private const int MIN_TEXT_LENGTH = 5;
    private const int MAX_TEXT_LENGTH = 1000;

    public CreateProductReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(MIN_RATING, MAX_RATING)
            .WithMessage($"Rating must be between {MIN_RATING} and {MAX_RATING}.");

        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Review text cannot be empty.");

        When(x => !string.IsNullOrWhiteSpace(x.Text), () =>
        {
            RuleFor(x => x.Text)
                .MinimumLength(MIN_TEXT_LENGTH)
                .WithMessage($"Review text must be at least {MIN_TEXT_LENGTH} characters long.");

            RuleFor(x => x.Text)
                .MaximumLength(MAX_TEXT_LENGTH)
                .WithMessage($"Review text must not exceed {MAX_TEXT_LENGTH} characters.");
        });
    }
}