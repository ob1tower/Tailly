using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.Services;
using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Application.Validators.SpecialistProfile;

public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
{
    private const int MAX_DESCRIPTION_LENGTH = 1000;
    private const decimal MIN_PRICE = 0;
    private const decimal MAX_PRICE = 1000000;

    public UpdateServiceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .Must(BeAValidServiceType)
            .WithMessage("Invalid service name. Allowed values: Walking, Boarding, Grooming, Training, Photoshoot.");

        RuleFor(x => x.Description)
            .MaximumLength(MAX_DESCRIPTION_LENGTH).WithMessage($"Description must not exceed {MAX_DESCRIPTION_LENGTH} characters.");

        RuleFor(x => x.Price)
            .GreaterThan(MIN_PRICE).WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(MAX_PRICE).WithMessage($"Price must not exceed {MAX_PRICE}.");

        RuleFor(x => x.PriceUnit)
            .NotEmpty().WithMessage("Price unit is required.")
            .Must(BeAValidPriceUnit)
            .WithMessage("Invalid price unit. Allowed values: Hour, Day, Service, Walk, Visit.");
    }

    private bool BeAValidServiceType(string value)
    {
        return Enum.TryParse<ServiceType>(value, true, out _);
    }

    private bool BeAValidPriceUnit(string value)
    {
        return Enum.TryParse<ServicePriceUnit>(value, true, out _);
    }
}