using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.Specialist;

namespace Tailly.SpecialistService.Application.Validators.SpecialistProfile;

public class GetSpecialistBySlugRequestValidator : AbstractValidator<GetSpecialistBySlugRequest>
{
    private const int MAX_SLUG_LENGTH = 100;

    public GetSpecialistBySlugRequestValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(MAX_SLUG_LENGTH)
            .WithMessage($"Slug must not exceed {MAX_SLUG_LENGTH} characters.");
    }
}