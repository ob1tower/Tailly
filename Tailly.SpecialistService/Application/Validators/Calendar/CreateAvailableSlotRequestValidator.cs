using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.Calendar;

namespace Tailly.SpecialistService.Application.Validators.Calendar;

public class CreateAvailableSlotRequestValidator : AbstractValidator<CreateAvailableSlotRequest>
{
    public CreateAvailableSlotRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .Must(x => DateOnly.TryParse(x, out _))
            .WithMessage("Invalid date.");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .Must(x => TimeOnly.TryParse(x, out _))
            .WithMessage("Invalid start time.");

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .Must(x => TimeOnly.TryParse(x, out _))
            .WithMessage("Invalid end time.");

        RuleFor(x => x.ServiceId)
            .NotEmpty()
            .WithMessage("Service id is required.");
    }
}