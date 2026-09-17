using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.Admin;
using Tailly.AuthService.Application.Errors;

namespace Tailly.AuthService.Application.Validators.Admin;

public class UpdateUserBlockStatusValidator : AbstractValidator<UpdateUserBlockStatusRequest>
{
    public UpdateUserBlockStatusValidator()
    {
        RuleFor(x => x.IsBlocked)
            .NotNull().WithMessage("IsBlocked field is required.");

        When(x => x.IsBlocked == true && x.IsPermanentBlock == false, () =>
        {
            RuleFor(x => x.BlockedUntil)
                .NotNull().WithMessage(AdminErrors.BlockDateRequired.Description)
                .GreaterThan(DateTime.UtcNow).WithMessage(AdminErrors.InvalidBlockDate.Description);
        });

        When(x => x.IsBlocked == true && x.IsPermanentBlock == true, () =>
        {
            RuleFor(x => x.BlockedUntil)
                .Null().WithMessage("BlockedUntil should not be provided for permanent block.");
        });

        RuleFor(x => x.BlockReason)
            .MaximumLength(500).WithMessage("Block reason cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.BlockReason));
    }
}