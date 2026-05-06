using FluentValidation;
using Tailly.BookingService.Application.Dtos.Requests;

namespace Tailly.BookingService.Application.Validators;

public class LeaveReviewRequestValidator : AbstractValidator<LeaveReviewRequest>
{
    private const int MAX_COMMENT_LENGTH = 2000;

    public LeaveReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(MAX_COMMENT_LENGTH).WithMessage($"Comment must not exceed {MAX_COMMENT_LENGTH} characters.");

        RuleFor(x => x.Photos)
            .NotNull().WithMessage("Photos collection cannot be null.");
    }
}