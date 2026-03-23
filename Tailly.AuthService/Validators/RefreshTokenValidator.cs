using FluentValidation;
using Tailly.AuthService.Dtos.Auth.Token;

namespace Tailly.AuthService.Validators;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}