using FluentValidation;
using Tailly.ShopService.Application.Dtos.Requests.Product;

namespace Tailly.ShopService.Application.Validators;

public class ReplyToReviewRequestValidator : AbstractValidator<ReplyToReviewRequest>
{
    private const int MAX_REPLY_TEXT_LENGTH = 1000;
    private const int MIN_REPLY_TEXT_LENGTH = 5;

    public ReplyToReviewRequestValidator()
    {
        RuleFor(x => x.Text)
                    .NotEmpty()
                    .WithMessage("Reply text cannot be empty.")
                    .MinimumLength(MIN_REPLY_TEXT_LENGTH)
                    .WithMessage($"Reply text must be at least {MIN_REPLY_TEXT_LENGTH} characters long.")
                    .MaximumLength(MAX_REPLY_TEXT_LENGTH)
                    .WithMessage($"Reply text must not exceed {MAX_REPLY_TEXT_LENGTH} characters.");
    }
}