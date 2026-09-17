using FluentValidation;
using Tailly.ClientProfileService.Application.Dtos.Requests;

namespace Tailly.ClientProfileService.Application.Validators;

public class UpdateClientProfileMainRequestValidator: AbstractValidator<UpdateClientProfileMainRequest>
{
    private const int MAX_NAME_LENGTH = 100;

    public UpdateClientProfileMainRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"First name must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(r => r.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"Last name must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(r => r.MiddleName)
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"Middle name must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(r => r.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL is too long.");
    }
}