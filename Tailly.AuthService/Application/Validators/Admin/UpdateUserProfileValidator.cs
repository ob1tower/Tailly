using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Admin;

namespace Tailly.AuthService.Application.Validators.Admin;

public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(100).WithMessage("Middle name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.SpecialistSlug)
            .MaximumLength(150).WithMessage("Specialist slug cannot exceed 150 characters.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Specialist slug can only contain lowercase letters, numbers and hyphens.")
            .When(x => !string.IsNullOrEmpty(x.SpecialistSlug));
    }
}