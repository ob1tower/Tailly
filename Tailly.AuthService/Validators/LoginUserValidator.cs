using FluentValidation;
using Tailly.AuthService.Dtos.Auth;

namespace Tailly.AuthService.Validators;

public class LoginUserValidator : AbstractValidator<LoginRequest>
{
    private const int MIN_EMAIL = 6;
    private const int MAX_EMAIL = 100;

    private const int MIN_PASS = 8;
    private const int MAX_PASS = 128;

    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
               .NotEmpty().WithMessage("Email is required.")
               .EmailAddress().WithMessage("Please enter a valid email address.")
               .Length(MIN_EMAIL, MAX_EMAIL)
               .WithMessage($"Email must be between {MIN_EMAIL} and {MAX_EMAIL} characters.");

        RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Length(MIN_PASS, MAX_PASS)
                .WithMessage($"Password must be between {MIN_PASS} and {MAX_PASS} characters.");
    }
}