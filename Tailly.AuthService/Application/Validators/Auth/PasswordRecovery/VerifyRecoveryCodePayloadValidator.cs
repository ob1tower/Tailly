using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;

namespace Tailly.AuthService.Application.Validators.Auth.PasswordRecovery;

public class VerifyRecoveryCodePayloadValidator : AbstractValidator<VerifyRecoveryCodePayload>
{
    public VerifyRecoveryCodePayloadValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(r => r.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.");
    }
}