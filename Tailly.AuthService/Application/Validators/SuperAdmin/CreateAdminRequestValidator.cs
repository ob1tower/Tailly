using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.SuperAdmin;

namespace Tailly.AuthService.Application.Validators.SuperAdmin;

public class CreateAdminRequestValidator : AbstractValidator<CreateAdminRequest>
{
    private const int MAX_EMAIL_LENGTH = 256;
    private const int MAX_NAME_LENGTH = 100;
    private const int MAX_PHONE_LENGTH = 32;

    public CreateAdminRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(MAX_EMAIL_LENGTH).WithMessage($"Email must not exceed {MAX_EMAIL_LENGTH} characters.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"First name must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"Last name must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"Middle name must not exceed {MAX_NAME_LENGTH} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.MiddleName));

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required.")
            .Must(BeAtLeast18YearsOld).WithMessage("User must be at least 18 years old.");

        RuleFor(x => x.Phone)
            .MaximumLength(MAX_PHONE_LENGTH).WithMessage($"Phone must not exceed {MAX_PHONE_LENGTH} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.");
    }

    private bool BeAtLeast18YearsOld(DateTime birthDate)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age >= 18;
    }
}