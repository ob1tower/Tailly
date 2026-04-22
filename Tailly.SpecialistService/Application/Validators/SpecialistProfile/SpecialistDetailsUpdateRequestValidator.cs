using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistProfile;

namespace Tailly.SpecialistService.Application.Validators.SpecialistProfile;

public class SpecialistDetailsUpdateRequestValidator : AbstractValidator<SpecialistDetailsUpdateRequest>
{
    public SpecialistDetailsUpdateRequestValidator()
    {
        RuleFor(x => x.ExperienceLabel)
            .NotEmpty().WithMessage("Experience label is required.")
            .MaximumLength(200);

        RuleFor(x => x.About)
            .NotEmpty().WithMessage("About is required.")
            .MaximumLength(2000);

        RuleFor(x => x.HousingType)
            .NotEmpty().WithMessage("Housing type is required.");

        RuleForEach(x => x.PetTypes)
            .NotEmpty().WithMessage("Pet type cannot be empty.");

        RuleForEach(x => x.PetSizes)
            .NotEmpty().WithMessage("Pet size cannot be empty.");

        RuleForEach(x => x.PetAges)
            .NotEmpty().WithMessage("Pet age cannot be empty.");

        RuleForEach(x => x.Advantages)
            .NotEmpty().WithMessage("Advantage cannot be empty.");

        RuleForEach(x => x.Services).ChildRules(service =>
        {
            service.RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Service name is required.")
                .MaximumLength(150);

            service.RuleFor(s => s.LocationLabel)
                .NotEmpty().WithMessage("Location label is required.")
                .MaximumLength(100);

            service.RuleFor(s => s.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
        });
    }
}