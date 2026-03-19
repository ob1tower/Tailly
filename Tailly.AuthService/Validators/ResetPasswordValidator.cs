using FluentValidation;
using Tailly.AuthService.Dtos.Auth;

namespace Tailly.AuthService.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    private const int MIN_PASS = 8;
    private const int MAX_PASS = 128;

    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must contain exactly 6 digits.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Length(MIN_PASS, MAX_PASS)
            .WithMessage($"New password must be between {MIN_PASS} and {MAX_PASS} characters.");
    }
}
