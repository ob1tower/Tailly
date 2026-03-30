using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.EmailChange;

namespace Tailly.AuthService.Application.Validators.Security;

public class ConfirmEmailChangeRequestValidator : AbstractValidator<ConfirmEmailChangeRequest>
{
    public ConfirmEmailChangeRequestValidator()
    {
        RuleFor(r => r.RequestId)
            .NotEmpty().WithMessage("RequestId is required.");

        RuleFor(r => r.NewEmail)
            .NotEmpty().WithMessage("New email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(r => r.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.");
    }
}