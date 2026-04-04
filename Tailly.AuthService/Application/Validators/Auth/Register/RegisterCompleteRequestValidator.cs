using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Register;

namespace Tailly.AuthService.Application.Validators.Auth.Register;

public class RegisterCompleteRequestValidator : AbstractValidator<RegisterCompleteRequest>
{
    public RegisterCompleteRequestValidator()
    {
        RuleFor(r => r.VerificationToken)
            .NotEmpty().WithMessage("VerificationToken is required.");

        RuleFor(r => r.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100);

        RuleFor(r => r.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100);

        RuleFor(r => r.CityName)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(r => r.CityId)
            .MaximumLength(50);
    }
}