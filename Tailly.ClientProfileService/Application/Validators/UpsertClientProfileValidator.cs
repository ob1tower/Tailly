using FluentValidation;
using Tailly.ClientProfileService.Application.Dtos.Requests;

namespace Tailly.ClientProfileService.Application.Validators;

public class UpsertClientProfileValidator : AbstractValidator<UpsertClientProfileRequest>
{
    public UpsertClientProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100)
            .When(x => x.MiddleName != null);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20)
            .Matches(@"^\+?[0-9]{10,15}$")
            .WithMessage("Invalid phone format.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.CityId)
            .Must(id => Guid.TryParse(id, out _))
            .When(x => x.CityId != null)
            .WithMessage("CityId must be a valid GUID.");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500);
    }
}