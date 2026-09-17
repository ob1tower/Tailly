using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Login;

namespace Tailly.AuthService.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    private const int MIN_PASSWORD_LENGTH = 8;

    public LoginRequestValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(l => l.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(MIN_PASSWORD_LENGTH).WithMessage($"The minimum length of the password is {MIN_PASSWORD_LENGTH} characters.");

        RuleFor(l => l.RequestedRole)
            .NotEmpty().WithMessage("RequestedRole is required.");
    }
}