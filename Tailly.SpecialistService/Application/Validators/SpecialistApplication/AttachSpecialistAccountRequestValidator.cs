using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

namespace Tailly.SpecialistService.Application.Validators.SpecialistApplication;

public class AttachSpecialistAccountRequestValidator : AbstractValidator<AttachSpecialistAccountRequest>
{
    public AttachSpecialistAccountRequestValidator()
    {
        RuleFor(x => x.ReviewedBy)
            .NotEmpty().WithMessage("ReviewedBy is required.");
    }
}