using Tailly.ClientProfileService.Core.Enums;

namespace Tailly.ClientProfileService.Application.Mappers;

public static class PetMapper
{
    public static string? MapPetType(PetType? type) => type switch
    {
        PetType.Dog => "dog",
        PetType.Cat => "cat",
        PetType.Bird => "bird",
        PetType.Rodent => "rodent",
        PetType.Rabbit => "rabbit",
        PetType.Reptile => "reptile",
        PetType.Fish => "fish",
        PetType.Amphibian => "amphibian",
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static bool TryParsePetType(string? value, out PetType? type)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "dog": type = PetType.Dog; return true;
            case "cat": type = PetType.Cat; return true;
            case "bird": type = PetType.Bird; return true;
            case "rodent": type = PetType.Rodent; return true;
            case "rabbit": type = PetType.Rabbit; return true;
            case "reptile": type = PetType.Reptile; return true;
            case "fish": type = PetType.Fish; return true;
            case "amphibian": type = PetType.Amphibian; return true;
            case null:
            case "": type = null; return true;
            default: type = null; return false;
        }
    }

    public static string? MapSize(PetSize? size) => size switch
    {
        PetSize.UpTo2Kg => "up_to_2kg",
        PetSize.Kg2To5 => "2_5kg",
        PetSize.Kg5To10 => "5_10kg",
        PetSize.Kg10To20 => "10_20kg",
        PetSize.Over20Kg => "over_20kg",
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
    };

    public static bool TryParseSize(string? value, out PetSize? size)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "up_to_2kg": size = PetSize.UpTo2Kg; return true;
            case "2_5kg": size = PetSize.Kg2To5; return true;
            case "5_10kg": size = PetSize.Kg5To10; return true;
            case "10_20kg": size = PetSize.Kg10To20; return true;
            case "over_20kg": size = PetSize.Over20Kg; return true;
            case null:
            case "": size = null; return true;
            default: size = null; return false;
        }
    }

    public static string? MapGender(PetGender? gender) => gender switch
    {
        PetGender.Male => "male",
        PetGender.Female => "female",
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null)
    };

    public static bool TryParseGender(string? value, out PetGender? gender)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "male": gender = PetGender.Male; return true;
            case "female": gender = PetGender.Female; return true;
            case null:
            case "": gender = null; return true;
            default: gender = null; return false;
        }
    }

    public static string? MapAttitude(PetAttitude? value) => value switch
    {
        PetAttitude.Friendly => "friendly",
        PetAttitude.Neutral => "neutral",
        PetAttitude.Aggressive => "aggressive",
        PetAttitude.Unknown => "unknown",
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static bool TryParseAttitude(string? value, out PetAttitude? result)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "friendly": result = PetAttitude.Friendly; return true;
            case "neutral": result = PetAttitude.Neutral; return true;
            case "aggressive": result = PetAttitude.Aggressive; return true;
            case "unknown": result = PetAttitude.Unknown; return true;
            case null:
            case "": result = null; return true;
            default: result = null; return false;
        }
    }

    public static string? MapHomeAlone(PetHomeAlone? value) => value switch
    {
        PetHomeAlone.Ok => "ok",
        PetHomeAlone.NotOk => "not_ok",
        PetHomeAlone.Unknown => "unknown",
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static bool TryParseHomeAlone(string? value, out PetHomeAlone? result)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "ok": result = PetHomeAlone.Ok; return true;
            case "not_ok": result = PetHomeAlone.NotOk; return true;
            case "unknown": result = PetHomeAlone.Unknown; return true;
            case null:
            case "": result = null; return true;
            default: result = null; return false;
        }
    }

    public static string? MapVaccinated(PetVaccinated? value) => value switch
    {
        PetVaccinated.Yes => "yes",
        PetVaccinated.No => "no",
        PetVaccinated.Unknown => "unknown",
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static bool TryParseVaccinated(string? value, out PetVaccinated? result)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "yes": result = PetVaccinated.Yes; return true;
            case "no": result = PetVaccinated.No; return true;
            case "unknown": result = PetVaccinated.Unknown; return true;
            case null:
            case "": result = null; return true;
            default: result = null; return false;
        }
    }
}