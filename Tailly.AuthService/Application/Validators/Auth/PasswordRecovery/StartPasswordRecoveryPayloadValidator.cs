using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.PasswordRecovery;

namespace Tailly.AuthService.Application.Validators.Auth.PasswordRecovery;

public class StartPasswordRecoveryPayloadValidator : AbstractValidator<StartPasswordRecoveryPayload>
{
    public StartPasswordRecoveryPayloadValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}