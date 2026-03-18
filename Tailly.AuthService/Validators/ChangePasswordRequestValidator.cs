using FluentValidation;
using Tailly.AuthService.Dtos.Auth;

namespace Tailly.AuthService.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    private const int MIN_PASS = 8;
    private const int MAX_PASS = 128;

    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.")
            .Length(MIN_PASS, MAX_PASS)
            .WithMessage($"Current password must be between {MIN_PASS} and {MAX_PASS} characters.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Length(MIN_PASS, MAX_PASS)
            .WithMessage($"New password must be between {MIN_PASS} and {MAX_PASS} characters.");
    }
}
