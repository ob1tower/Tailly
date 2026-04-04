using FluentValidation;
using Tailly.ClientProfileService.Application.Dtos.Requests;

namespace Tailly.ClientProfileService.Application.Validators;

public class UpdateClientProfileContactsRequestValidator: AbstractValidator<UpdateClientProfileContactsRequest>
{
    private const int MAX_CITY_LENGTH = 200;
    private const int MAX_PHONE_LENGTH = 20;

    public UpdateClientProfileContactsRequestValidator()
    {
        RuleFor(r => r.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(MAX_PHONE_LENGTH)
            .Matches(@"^\+?[0-9]{10,15}$")
            .WithMessage("Invalid phone format.");

        RuleFor(r => r.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(MAX_CITY_LENGTH).WithMessage($"City must not exceed {MAX_CITY_LENGTH} characters.");

        RuleFor(r => r.CityId)
            .MaximumLength(50)
            .When(r => r.CityId != null);
    }
}