using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

namespace Tailly.SpecialistService.Application.Validators.SpecialistApplication;

public class AssignInterviewRequestValidator : AbstractValidator<AssignInterviewRequest>
{
    public AssignInterviewRequestValidator()
    {
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Note is required.")
            .MaximumLength(500);

        RuleFor(x => x.InterviewDate)
            .NotNull()
            .WithMessage("InterviewDate is required.");
    }
}