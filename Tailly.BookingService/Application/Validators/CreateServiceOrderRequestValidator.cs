using FluentValidation;
using Tailly.BookingService.Application.Dtos.Requests;

namespace Tailly.BookingService.Application.Validators;

public class CreateServiceOrderRequestValidator : AbstractValidator<CreateServiceOrderRequest>
{
    private const int MAX_SERVICE_TITLE_LENGTH = 300;
    private const int MAX_COMMENT_LENGTH = 1000;

    public CreateServiceOrderRequestValidator()
    {
        RuleFor(x => x.SpecialistId)
            .NotEmpty().WithMessage("SpecialistId is required.");

        RuleFor(x => x.PetId)
            .NotEmpty().WithMessage("PetId is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("ServiceId is required.");

        RuleFor(x => x.ServiceTitle)
            .NotEmpty().WithMessage("ServiceTitle is required.")
            .MaximumLength(MAX_SERVICE_TITLE_LENGTH).WithMessage($"The maximum length of the service title is {MAX_SERVICE_TITLE_LENGTH} characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.StartAt)
            .NotEmpty().WithMessage("StartAt is required.");

        RuleFor(x => x.Comment)
            .MaximumLength(MAX_COMMENT_LENGTH).When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage($"The maximum length of the comment is {MAX_COMMENT_LENGTH} characters.");
    }
}