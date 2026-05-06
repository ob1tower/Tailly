using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.Reviews;

namespace Tailly.SpecialistService.Application.Validators.SpecialistProfile;

public class ReviewReplyRequestValidator : AbstractValidator<ReviewReplyRequest>
{
    private const int MAX_REPLY_LENGTH = 2000;

    public ReviewReplyRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Reply text is required.")
            .MaximumLength(MAX_REPLY_LENGTH).WithMessage($"Reply text must not exceed {MAX_REPLY_LENGTH} characters.");
    }
}