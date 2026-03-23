using FluentValidation;
using Tailly.AuthService.Dtos.Auth.Password;

namespace Tailly.AuthService.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
