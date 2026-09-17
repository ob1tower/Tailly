using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

namespace Tailly.SpecialistService.Application.Validators.SpecialistApplication;

public class CreateSpecialistApplicationRequestValidator : AbstractValidator<CreateSpecialistApplicationRequest>
{
    public CreateSpecialistApplicationRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.About)
            .NotEmpty().WithMessage("About is required.")
            .MaximumLength(2000);

        RuleFor(x => x.Questionnaire)
            .NotNull().WithMessage("Questionnaire is required.");

        RuleFor(x => x.Questionnaire!.ExperienceYears)
            .NotEmpty().WithMessage("ExperienceYears is required.");

        RuleFor(x => x.Questionnaire!.ServiceFormats)
            .NotEmpty().WithMessage("ServiceFormats is required. At least one service format must be selected.");

        RuleFor(x => x.Questionnaire!.AnimalTypes)
            .NotEmpty().WithMessage("AnimalTypes is required. At least one animal type must be selected.");

        RuleFor(x => x.Questionnaire!.HousingType)
            .NotEmpty().WithMessage("HousingType is required.");
    }
}