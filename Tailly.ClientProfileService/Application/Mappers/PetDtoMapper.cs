using Tailly.ClientProfileService.Application.Dtos.Requests;
using Tailly.ClientProfileService.Application.Dtos.Responses;
using Tailly.ClientProfileService.Core.Enums;
using Tailly.ClientProfileService.Core.Models;

namespace Tailly.ClientProfileService.Application.Mappers;

public static class PetDtoMapper
{
    public static PetResponse ToResponse(Pet p)
    {
        return new PetResponse
        {
            Id = p.Id.ToString(),
            Name = p.Name,
            PhotoUrl = p.PhotoUrl,
            Type = PetMapper.MapPetType(p.Type),
            BreedId = p.BreedId?.ToString(),
            AgeYears = p.AgeYears,
            AgeMonths = p.AgeMonths,
            Size = PetMapper.MapSize(p.Size),
            Gender = PetMapper.MapGender(p.Gender),
            ToOtherPets = PetMapper.MapAttitude(p.ToOtherPets),
            ToKidsUnder10 = PetMapper.MapAttitude(p.ToKidsUnder10),
            StaysHomeAlone = PetMapper.MapHomeAlone(p.StaysHomeAlone),
            Vaccinated = PetMapper.MapVaccinated(p.Vaccinated),
            Notes = p.Notes
        };
    }

    public static Pet ToModel(UpsertPetRequest request)
    {
        PetType? type = null;
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (!PetMapper.TryParsePetType(request.Type, out var parsed))
                throw new ArgumentException("Invalid pet type");

            type = parsed;
        }

        PetMapper.TryParseSize(request.Size, out var size);
        PetMapper.TryParseGender(request.Gender, out var gender);
        PetMapper.TryParseAttitude(request.ToOtherPets, out var toPets);
        PetMapper.TryParseAttitude(request.ToKidsUnder10, out var toKids);
        PetMapper.TryParseHomeAlone(request.StaysHomeAlone, out var homeAlone);
        PetMapper.TryParseVaccinated(request.Vaccinated, out var vaccinated);

        Guid? breedId = null;
        if (!string.IsNullOrWhiteSpace(request.BreedId))
        {
            if (!Guid.TryParse(request.BreedId, out var parsed))
                throw new ArgumentException("Invalid breedId");

            breedId = parsed;
        }

        return new Pet
        {
            Name = request.Name,
            PhotoUrl = request.PhotoUrl,
            Type = type,
            BreedId = breedId,
            AgeYears = request.AgeYears,
            AgeMonths = request.AgeMonths,
            Size = size,
            Gender = gender,
            ToOtherPets = toPets,
            ToKidsUnder10 = toKids,
            StaysHomeAlone = homeAlone,
            Vaccinated = vaccinated,
            Notes = request.Notes ?? ""
        };
    }
}