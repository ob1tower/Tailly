using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Admin;

namespace Tailly.AuthService.Application.Validators.Admin;

public class UpdateAdminProfileRequestValidator : AbstractValidator<UpdateAdminProfileRequest>
{
    private const int MAX_NAME_LENGTH = 100;
    private const int MAX_PHONE_LENGTH = 20;

    public UpdateAdminProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"First name must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"Last name must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"Middle name must not exceed {MAX_NAME_LENGTH} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.MiddleName));

        RuleFor(x => x.Phone)
            .MaximumLength(MAX_PHONE_LENGTH).WithMessage($"Phone must not exceed {MAX_PHONE_LENGTH} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.BirthDate)
            .Must(BeAtLeast18YearsOld).WithMessage("You must be at least 18 years old.")
            .When(x => x.BirthDate.HasValue);
    }

    private bool BeAtLeast18YearsOld(DateTime? birthDate)
    {
        if (!birthDate.HasValue) return true;
        return birthDate.Value.AddYears(18) <= DateTime.UtcNow;
    }
}