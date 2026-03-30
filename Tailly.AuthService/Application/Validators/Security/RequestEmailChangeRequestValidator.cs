using FluentValidation;
using Tailly.AuthService.Application.Dtos.Requests.EmailChange;

namespace Tailly.AuthService.Application.Validators.Security;

public class RequestEmailChangeRequestValidator : AbstractValidator<RequestEmailChangeRequest>
{
    private const int MAX_EMAIL_LENGTH = 256;

    public RequestEmailChangeRequestValidator()
    {
        RuleFor(r => r.NewEmail)
            .NotEmpty().WithMessage("New email is required.")
            .MaximumLength(MAX_EMAIL_LENGTH).WithMessage($"Email must not exceed {MAX_EMAIL_LENGTH} characters.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}