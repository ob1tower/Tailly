using FluentValidation;
using Tailly.AuthService.Dtos.Auth.Register;

namespace Tailly.AuthService.Validators;

public class ChangeEmailValidator : AbstractValidator<ChangeEmailRequest>
{
    private const int MIN_EMAIL = 6;
    private const int MAX_EMAIL = 100;

    public ChangeEmailValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("New email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .Length(MIN_EMAIL, MAX_EMAIL)
            .WithMessage($"Email must be between {MIN_EMAIL} and {MAX_EMAIL} characters.");
    }
}