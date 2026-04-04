namespace Tailly.ClientProfileService.Core.Enums;

public enum PetType
{
    Dog = 1,
    Cat = 2,
    Bird = 3,
    Rodent = 4,
    Rabbit = 5,
    Reptile = 6,
    Fish = 7,
    Amphibian = 8
}

public enum PetSize
{
    UpTo2Kg = 1,
    Kg2To5 = 2,
    Kg5To10 = 3,
    Kg10To20 = 4,
    Over20Kg = 5
}

public enum PetGender
{
    Male = 1,
    Female = 2
}

public enum PetAttitude
{
    Friendly = 1,
    Neutral = 2,
    Aggressive = 3,
    Unknown = 4
}

public enum PetHomeAlone
{
    Ok = 1,
    NotOk = 2,
    Unknown = 3
}

public enum PetVaccinated
{
    Yes = 1,
    No = 2,
    Unknown = 3
}