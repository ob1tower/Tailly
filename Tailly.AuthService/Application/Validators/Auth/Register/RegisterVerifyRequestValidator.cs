using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Register;

namespace Tailly.AuthService.Application.Validators.Auth.Register;

public class RegisterVerifyRequestValidator : AbstractValidator<RegisterVerifyRequest>
{
    public RegisterVerifyRequestValidator()
    {
        RuleFor(r => r.RegistrationId)
            .NotEmpty().WithMessage("RegistrationId is required.");

        RuleFor(r => r.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.");
    }
}