using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.AccountDeletion;
using Tailly.AuthService.Application.Errors;

namespace Tailly.AuthService.Application.Validators.AccountDeletion;

public class RestoreRequestValidator : AbstractValidator<RestoreAccountByTokenRequest>
{
    private const int TokenMinLength = 32;
    private const int TokenMaxLength = 128;

    public RestoreRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(AuthErrors.InvalidVerificationToken.Description)
            .MinimumLength(TokenMinLength).WithMessage("Restore token is too short.")
            .MaximumLength(TokenMaxLength).WithMessage("Restore token is too long.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Restore token contains invalid characters.");
    }
}