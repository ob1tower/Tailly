using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

namespace Tailly.SpecialistService.Application.Validators.SpecialistApplication;

public class ApproveApplicationRequestValidator : AbstractValidator<ApproveApplicationRequest>
{
    public ApproveApplicationRequestValidator()
    {
        RuleFor(x => x.ReviewComment)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.ReviewComment))
            .WithMessage("ReviewComment cannot exceed 1000 characters.");
    }
}