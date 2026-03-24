using FluentValidation;
using Tailly.AuthService.Dtos.Auth.Email;

namespace Tailly.AuthService.Validators;

public class ConfirmEmailChangeValidator : AbstractValidator<ConfirmEmailChangeRequest>
{
    private const int MIN_EMAIL = 6;
    private const int MAX_EMAIL = 100;

    private const int MIN_CODE = 4;
    private const int MAX_CODE = 10;

    public ConfirmEmailChangeValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("New email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .Length(MIN_EMAIL, MAX_EMAIL)
            .WithMessage($"Email must be between {MIN_EMAIL} and {MAX_EMAIL} characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(MIN_CODE, MAX_CODE)
            .WithMessage($"Code must be between {MIN_CODE} and {MAX_CODE} characters.");
    }
}