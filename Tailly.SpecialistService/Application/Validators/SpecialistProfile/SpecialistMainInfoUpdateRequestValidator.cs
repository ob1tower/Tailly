using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistProfile;

namespace Tailly.SpecialistService.Application.Validators.SpecialistProfile;

public class SpecialistMainInfoUpdateRequestValidator : AbstractValidator<SpecialistMainInfoUpdateRequest>
{
    public SpecialistMainInfoUpdateRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("District is required.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(30);
    }
}