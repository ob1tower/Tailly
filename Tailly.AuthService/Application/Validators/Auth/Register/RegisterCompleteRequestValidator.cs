using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Register;

namespace Tailly.AuthService.Application.Validators.Auth.Register;

public class RegisterCompleteRequestValidator : AbstractValidator<RegisterCompleteRequest>
{
    public RegisterCompleteRequestValidator()
    {
        RuleFor(r => r.RegistrationId)
            .NotEmpty().WithMessage("RegistrationId is required.");

        RuleFor(r => r.VerificationToken)
            .NotEmpty().WithMessage("VerificationToken is required.");
    }
}