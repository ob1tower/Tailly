using FluentValidation;
using Tailly.BookingService.Application.Dtos.Requests;

namespace Tailly.BookingService.Application.Validators;

public class CreateServiceOrderRequestValidator : AbstractValidator<CreateServiceOrderRequest>
{
    private const int MAX_COMMENT_LENGTH = 1000;

    public CreateServiceOrderRequestValidator()
    {
        RuleFor(x => x.SpecialistId)
            .NotEmpty().WithMessage("SpecialistId is required.");

        RuleFor(x => x.PetId)
            .NotEmpty().WithMessage("PetId is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("ServiceId is required.");

        RuleFor(x => x.StartAt)
            .NotEmpty().WithMessage("StartAt is required.");

        RuleFor(x => x.Comment)
            .MaximumLength(MAX_COMMENT_LENGTH).When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage($"The maximum length of the comment is {MAX_COMMENT_LENGTH} characters.");
    }
}