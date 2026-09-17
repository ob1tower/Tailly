using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

namespace Tailly.SpecialistService.Application.Validators.SpecialistApplication;

public class RejectApplicationRequestValidator : AbstractValidator<RejectApplicationRequest>
{
    public RejectApplicationRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(500);
    }
}