using FluentValidation;
using Tailly.AuthService.Dtos.Auth;

namespace Tailly.AuthService.Validators;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    private const int CODE_LENGTH = 6;

    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(CODE_LENGTH)
            .WithMessage($"Verification code must be {CODE_LENGTH} characters.");
    }
}
