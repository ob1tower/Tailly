using FluentValidation;
using Tailly.SpecialistService.Application.Dtos.Requests.Specialist;
using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Application.Validators.SpecialistProfile;

public class UpdateSpecialistDetailsRequestValidator : AbstractValidator<UpdateSpecialistDetailsRequest>
{
    private const int MAX_ABOUT_LENGTH = 2000;
    private const int MAX_GALLERY_ITEMS = 10;

    public UpdateSpecialistDetailsRequestValidator()
    {
        RuleFor(x => x.About)
            .NotEmpty().WithMessage("About section is required.")
            .MaximumLength(MAX_ABOUT_LENGTH).WithMessage($"About must not exceed {MAX_ABOUT_LENGTH} characters.");

        RuleFor(x => x.HousingType)
            .NotEmpty().WithMessage("Housing type is required.")
            .Must(BeAValidHousingType)
            .WithMessage("Invalid housing type. Allowed values: Apartment, House, Townhouse, Other.");

        RuleFor(x => x.HasChildrenUnderTen)
            .NotEmpty().WithMessage("Children presence is required.")
            .Must(BeAValidChildrenPresence)
            .WithMessage("Invalid children presence value. Allowed values: Yes, No, Sometimes.");

        RuleForEach(x => x.PetSizes)
            .Must(BeAValidPetSize)
            .WithMessage("Invalid value in PetSizes. Allowed values: UpTo2Kg, Kg2To5, Kg5To10, Kg10To20, Over20Kg.");

        RuleForEach(x => x.PetAges)
            .Must(BeAValidPetAge)
            .WithMessage("Invalid value in PetAges. Allowed values: Baby, Young, Adult, Senior.");

        RuleForEach(x => x.PetTypes)
            .Must(BeAValidPetType)
            .WithMessage("Invalid value in PetTypes. Allowed values: Dog, Cat, Bird, Rodent, Rabbit, Reptile, Fish, Amphibian.");

        RuleFor(x => x.SpecialistGallery)
            .Must(gallery => gallery == null || gallery.Count <= MAX_GALLERY_ITEMS)
            .WithMessage($"You can upload maximum {MAX_GALLERY_ITEMS} gallery images.");
    }

    private bool BeAValidHousingType(string value)
    {
        return Enum.TryParse<HousingType>(value, true, out _);
    }

    private bool BeAValidChildrenPresence(string value)
    {
        return Enum.TryParse<ChildrenPolicy>(value, true, out _);
    }

    private bool BeAValidPetSize(string value)
    {
        if (Enum.TryParse<PetSize>(value, true, out _))
            return true;

        return value.ToLowerInvariant() switch
        {
            "upto2kg" or "up_to_2kg" or "2kg" => true,
            "kg2to5" or "2_5kg" or "2-5kg" => true,
            "kg5to10" or "5_10kg" or "5-10kg" => true,
            "kg10to20" or "10_20kg" or "10-20kg" => true,
            "over20kg" or "over_20kg" or ">20kg" or "20kg+" => true,
            _ => false
        };
    }

    private bool BeAValidPetAge(string value)
    {
        return Enum.TryParse<PetAge>(value, true, out _);
    }

    private bool BeAValidPetType(string value)
    {
        return Enum.TryParse<PetType>(value, true, out _);
    }
}