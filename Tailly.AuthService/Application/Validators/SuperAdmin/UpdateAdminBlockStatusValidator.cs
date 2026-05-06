using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.SuperAdmin;

namespace Tailly.AuthService.Application.Validators.SuperAdmin;

public class UpdateAdminBlockStatusValidator : AbstractValidator<UpdateAdminBlockStatusPayload>
{
    private const int MAX_BLOCK_REASON_LENGTH = 500;

    public UpdateAdminBlockStatusValidator()
    {
        RuleFor(x => x.BlockReason)
            .MaximumLength(MAX_BLOCK_REASON_LENGTH)
            .WithMessage($"Block reason must not exceed {MAX_BLOCK_REASON_LENGTH} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BlockReason));

        When(x => x.IsBlocked == true, () =>
        {
            RuleFor(x => x.BlockReason)
                .NotEmpty()
                .WithMessage("Block reason is required when blocking an administrator.");

            When(x => x.IsPermanentBlock == false, () =>
            {
                RuleFor(x => x.BlockedUntil)
                    .NotNull()
                    .WithMessage("BlockedUntil is required for temporary block.")
                    .Must(BeInTheFuture)
                    .WithMessage("BlockedUntil must be in the future.");
            });
        });
    }

    private bool BeInTheFuture(DateTime? date)
    {
        return date.HasValue && date.Value > DateTime.UtcNow;
    }
}