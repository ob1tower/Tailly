using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.Specialist;

namespace Tailly.SpecialistService.Application.Validators.SpecialistProfile;

public class UpdateSpecialistMainInfoRequestValidator : AbstractValidator<UpdateSpecialistMainInfoRequest>
{
    private const int MAX_NAME_LENGTH = 100;
    private const int MAX_CITY_LENGTH = 150;
    private const int MAX_DISTRICT_LENGTH = 150;
    private const int MAX_PHONE_LENGTH = 20;

    public UpdateSpecialistMainInfoRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"FirstName must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"LastName must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(MAX_NAME_LENGTH).WithMessage($"MiddleName must not exceed {MAX_NAME_LENGTH} characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(MAX_CITY_LENGTH).WithMessage($"City must not exceed {MAX_CITY_LENGTH} characters.");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("District is required.")
            .MaximumLength(MAX_DISTRICT_LENGTH).WithMessage($"District must not exceed {MAX_DISTRICT_LENGTH} characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(MAX_PHONE_LENGTH).WithMessage($"Phone must not exceed {MAX_PHONE_LENGTH} characters.");
    }
}