using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Token;

namespace Tailly.AuthService.Application.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(r => r.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}