using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;

namespace Tailly.AuthService.Application.Validators.Auth.PasswordRecovery;

public class ResetPasswordPayloadValidator : AbstractValidator<ResetPasswordPayload>
{
    private const int MIN_PASSWORD_LENGTH = 8;
    private const int MAX_PASSWORD_LENGTH = 100;

    public ResetPasswordPayloadValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(r => r.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.");

        RuleFor(r => r.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(MIN_PASSWORD_LENGTH).WithMessage($"The minimum length of the password is {MIN_PASSWORD_LENGTH} characters.")
            .MaximumLength(MAX_PASSWORD_LENGTH).WithMessage($"The maximum length of the password is {MAX_PASSWORD_LENGTH} characters.");
    }
}