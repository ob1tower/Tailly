using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Application.Mappers;

public static class SpecialistEnumMapper
{
    public static string MapHousingType(HousingType value) => value switch
    {
        HousingType.Apartment => "apartment",
        HousingType.House => "house",
        HousingType.Townhouse => "townhouse",
        HousingType.Other => "other",
        _ => "other"
    };

    public static string MapChildrenPresence(ChildrenPresence value) => value switch
    {
        ChildrenPresence.Yes => "yes",
        ChildrenPresence.No => "no",
        ChildrenPresence.Sometimes => "sometimes",
        _ => "no"
    };

    public static string MapPriceUnit(PriceUnit value) => value switch
    {
        PriceUnit.Hour => "hour",
        PriceUnit.Day => "day",
        PriceUnit.Service => "service",
        PriceUnit.Walk => "walk",
        PriceUnit.Visit => "visit",
        _ => "hour"
    };

    public static string MapPetType(PetType value) => value switch
    {
        PetType.Dog => "dog",
        PetType.Cat => "cat",
        PetType.Bird => "bird",
        PetType.Rodent => "rodent",
        PetType.Rabbit => "rabbit",
        PetType.Reptile => "reptile",
        PetType.Fish => "fish",
        PetType.Amphibian => "amphibian",
        _ => "dog"
    };

    public static string MapPetSize(PetSize value) => value switch
    {
        PetSize.UpTo2Kg => "up_to_2kg",
        PetSize.Kg2To5 => "2_5kg",         
        PetSize.Kg5To10 => "5_10kg",
        PetSize.Kg10To20 => "10_20kg",
        PetSize.Over20Kg => "over_20kg",
        _ => "5_10kg"
    };

    public static string MapPetAge(PetAge value) => value switch
    {
        PetAge.Baby => "baby",
        PetAge.Young => "young",
        PetAge.Adult => "adult",
        PetAge.Senior => "senior",
        _ => "adult"
    };

    public static string MapExperienceUnit(ExperienceUnit value) => value switch
    {
        ExperienceUnit.Years => "years",
        ExperienceUnit.Months => "months",
        _ => "years"
    };

    public static string MapServiceType(ServiceType value) => value switch
    {
        ServiceType.Walking => "walking",
        ServiceType.Boarding => "boarding",
        ServiceType.Grooming => "grooming",
        ServiceType.Training => "training",
        ServiceType.Photoshoot => "photoshoot",
        _ => "walking"
    };

    public static string MapBookingMode(SpecialistBookingMode mode) => mode switch
    {
        SpecialistBookingMode.FixedSlot => "fixed_slot",
        SpecialistBookingMode.TimeRange => "time_range",
        SpecialistBookingMode.MultiDayStay => "multi_day_stay",
        SpecialistBookingMode.OpenRequest => "open_request",
        _ => "fixed_slot"
    };

    public static string MapApplicationStatus(SpecialistApplicationStatus value) => value switch
    {
        SpecialistApplicationStatus.Pending => "pending_review",
        SpecialistApplicationStatus.InterviewScheduled => "interview_assigned",
        SpecialistApplicationStatus.Approved => "approved",
        SpecialistApplicationStatus.Rejected => "rejected",
        _ => "pending_review"
    };

    public static SpecialistSort ParseSort(string? sort) => sort switch
    {
        "price-asc" => SpecialistSort.PriceAsc,
        "price-desc" => SpecialistSort.PriceDesc,
        "rating" => SpecialistSort.RatingDesc,
        _ => SpecialistSort.RatingDesc
    };

    public static SpecialistApplicationStatus ParseApplicationStatus(string? status) => status switch
    {
        "pending_review" => SpecialistApplicationStatus.Pending,
        "interview_assigned" => SpecialistApplicationStatus.InterviewScheduled,
        "approved" => SpecialistApplicationStatus.Approved,
        "rejected" => SpecialistApplicationStatus.Rejected,
        _ => SpecialistApplicationStatus.Pending
    };

    public static ExperienceUnit? ParseExperienceUnit(string? value) => value?.ToLowerInvariant() switch
    {
        "years" => ExperienceUnit.Years,
        "months" => ExperienceUnit.Months,
        _ => null
    };

    public static HousingType ParseHousingType(string? value) => value?.ToLowerInvariant() switch
    {
        "house" => HousingType.House,
        "townhouse" => HousingType.Townhouse,
        "other" => HousingType.Other,
        _ => HousingType.Apartment
    };

    public static ChildrenPresence ParseChildrenPresence(string? value) => value?.ToLowerInvariant() switch
    {
        "yes" => ChildrenPresence.Yes,
        "sometimes" => ChildrenPresence.Sometimes,
        _ => ChildrenPresence.No
    };

    public static PetType ParsePetType(string? value) => value?.ToLowerInvariant() switch
    {
        "cat" => PetType.Cat,
        "bird" => PetType.Bird,
        "rodent" => PetType.Rodent,
        "rabbit" => PetType.Rabbit,
        "reptile" => PetType.Reptile,
        "fish" => PetType.Fish,
        "amphibian" => PetType.Amphibian,
        _ => PetType.Dog
    };

    public static PetSize ParsePetSize(string? value) => value?.ToLowerInvariant() switch
    {
        "up_to_2kg" => PetSize.UpTo2Kg,
        "2_5kg" => PetSize.Kg2To5,
        "5_10kg" => PetSize.Kg5To10,
        "10_20kg" => PetSize.Kg10To20,
        "over_20kg" => PetSize.Over20Kg,
        _ => PetSize.Kg5To10
    };

    public static PetAge ParsePetAge(string? value) => value?.ToLowerInvariant() switch
    {
        "baby" => PetAge.Baby,
        "young" => PetAge.Young,
        "senior" => PetAge.Senior,
        _ => PetAge.Adult
    };

    public static ServiceType ParseServiceType(string? name) => name?.ToLowerInvariant() switch
    {
        null => ServiceType.Walking,                   

        var n when n.Contains("walk") => ServiceType.Walking,
        var n when n.Contains("board") => ServiceType.Boarding,
        var n when n.Contains("groom") => ServiceType.Grooming,
        var n when n.Contains("train") => ServiceType.Training,
        var n when n.Contains("photo") => ServiceType.Photoshoot,

        _ => ServiceType.Walking
    };

    public static PriceUnit ParsePriceUnit(string? value) => value?.ToLowerInvariant() switch
    {
        "day" => PriceUnit.Day,
        "service" => PriceUnit.Service,
        "walk" => PriceUnit.Walk,
        "visit" => PriceUnit.Visit,
        _ => PriceUnit.Hour
    };
}