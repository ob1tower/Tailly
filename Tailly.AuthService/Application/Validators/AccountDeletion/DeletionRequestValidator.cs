using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.AccountDeletion;

namespace Tailly.AuthService.Application.Validators.AccountDeletion;

public class DeletionRequestValidator : AbstractValidator<AccountDeletionRequest>
{
    public DeletionRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}