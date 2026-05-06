namespace Tailly.SpecialistService.Core.Enums;

public enum HousingType
{
    Apartment = 1,
    House,
    Townhouse,
    Other
}

public enum ChildrenPolicy
{
    Yes = 1,
    No,
    Sometimes
}

public enum ServicePriceUnit
{
    Hour = 1,
    Day,
    Service,
    Walk,
    Visit
}

public enum PetType
{
    Dog = 1,
    Cat,
    Bird,
    Rodent,
    Rabbit,
    Reptile,
    Fish,
    Amphibian
}

public enum PetSize
{
    UpTo2Kg = 1,
    Kg2To5,
    Kg5To10,
    Kg10To20,
    Over20Kg
}

public enum PetAge
{
    Baby = 1,
    Young,
    Adult,
    Senior
}

public enum ExperienceUnit
{
    Years = 1,
    Months
}

public enum BookingMode
{
    FixedSlot = 1,
    TimeRange,
    MultiDayStay,
    OpenRequest
}

public enum SpecialistSort
{
    RatingDesc = 1,
    PriceAsc,
    PriceDesc
}

public enum ServiceType
{
    Walking = 1,
    Boarding,
    Grooming,
    Training,
    Photoshoot
}

public enum CalendarDayStatus
{
    Available = 1,
    PartiallyBooked
}