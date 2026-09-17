using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.Calendar;
using Tailly.SpecialistService.Application.Mappers;

namespace Tailly.SpecialistService.Application.Validators.Calendar;

public class CreateDayOverrideRequestValidator : AbstractValidator<CreateDayOverrideRequest>
{
    public CreateDayOverrideRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .Must(x => DateOnly.TryParse(x, out _))
            .WithMessage("Invalid date.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(x =>
            {
                try
                {
                    SpecialistEnumMapper.ParseCalendarDayStatus(x);
                    return true;
                }
                catch
                {
                    return false;
                }
            })
            .WithMessage("Invalid calendar day status.");
    }
}