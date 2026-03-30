using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Register;

namespace Tailly.AuthService.Application.Validators.Auth.Register;

public class RegisterStartRequestValidator : AbstractValidator<RegisterStartRequest>
{
    private const int MIN_PASSWORD_LENGTH = 8;
    private const int MAX_PASSWORD_LENGTH = 100;
    private const int MAX_EMAIL_LENGTH = 256;

    public RegisterStartRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(MAX_EMAIL_LENGTH).WithMessage($"Email must not exceed {MAX_EMAIL_LENGTH} characters.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(MIN_PASSWORD_LENGTH).WithMessage($"The minimum length of the password is {MIN_PASSWORD_LENGTH} characters.")
            .MaximumLength(MAX_PASSWORD_LENGTH).WithMessage($"The maximum length of the password is {MAX_PASSWORD_LENGTH} characters.");
    }
}