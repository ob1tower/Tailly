using FluentValidation;
using Tailly.ClientProfileService.Application.Dtos.Requests;
using Tailly.ClientProfileService.Application.Mappers;

namespace Tailly.ClientProfileService.Application.Validators;

public class UpsertPetValidator : AbstractValidator<UpsertPetRequest>
{
    public UpsertPetValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .Must(v => v == null || PetMapper.TryParsePetType(v, out _))
            .WithMessage("Invalid pet type.");

        RuleFor(x => x.Size)
            .Must(v => v == null || PetMapper.TryParseSize(v, out _))
            .WithMessage("Invalid size.");

        RuleFor(x => x.Gender)
            .Must(v => v == null || PetMapper.TryParseGender(v, out _));

        RuleFor(x => x.ToOtherPets)
            .Must(v => v == null || PetMapper.TryParseAttitude(v, out _));

        RuleFor(x => x.ToKidsUnder10)
            .Must(v => v == null || PetMapper.TryParseAttitude(v, out _));

        RuleFor(x => x.StaysHomeAlone)
            .Must(v => v == null || PetMapper.TryParseHomeAlone(v, out _));

        RuleFor(x => x.Vaccinated)
            .Must(v => v == null || PetMapper.TryParseVaccinated(v, out _));

        RuleFor(x => x.BreedId)
            .Must(v => v == null || Guid.TryParse(v, out _))
            .WithMessage("Invalid breedId.");

        RuleFor(x => x.AgeYears)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AgeMonths)
            .InclusiveBetween(0, 11);

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}