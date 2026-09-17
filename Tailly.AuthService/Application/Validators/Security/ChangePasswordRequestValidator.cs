using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests;

namespace Tailly.AuthService.Application.Validators.Security;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    private const int MIN_PASSWORD_LENGTH = 8;
    private const int MAX_PASSWORD_LENGTH = 100;

    public ChangePasswordRequestValidator()
    {
        RuleFor(r => r.OldPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(r => r.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(MIN_PASSWORD_LENGTH).WithMessage($"The minimum length of the password is {MIN_PASSWORD_LENGTH} characters.")
            .MaximumLength(MAX_PASSWORD_LENGTH).WithMessage($"The maximum length of the password is {MAX_PASSWORD_LENGTH} characters.");
    }
}