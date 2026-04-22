using Tailly.SpecialistService.Application.Dtos.Responses.BookingPolicy;
using Tailly.SpecialistService.Application.Dtos.Responses.Calendar;
using Tailly.SpecialistService.Application.Dtos.Responses.Common;
using Tailly.SpecialistService.Application.Dtos.Responses.Short;
using Tailly.SpecialistService.Application.Dtos.Responses.SpecialistProfile;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Calendar;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Application.Mappers;

public static class SpecialistMapper
{
    public static SpecialistProfileResponse ToProfileResponse(this Specialist model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var details = model.Details ?? new Details();

        return new SpecialistProfileResponse
        {
            Id = model.Id,
            Slug = model.Slug,

            Main = model.ToMainInfoResponse(),
            Stats = model.ToStatsResponse(),
            Calendar = model.ToCalendarResponse(),
            Details = model.ToDetailsResponse(),

            Services = model.Services.Select(s => s.ToServiceResponse()).ToList(),
            Reviews = model.Reviews.Select(r => r.ToReviewResponse()).ToList(),

            SpecialistGallery = model.Gallery.Select(g => g.ToGalleryResponse()).ToList(),
            PetGallery = new List<GalleryResponse>() // пока пусто, можно потом сделать отдельную коллекцию
        };
    }

    // ==================== Простые мапперы ====================

    private static SpecialistMainInfoResponse ToMainInfoResponse(this Specialist m) => new()
    {
        FirstName = m.FirstName,
        LastName = m.LastName,
        MiddleName = m.MiddleName,
        City = m.City,
        District = m.District ?? "",
        Phone = m.Phone ?? "",
        Email = m.Email,
        AvatarUrl = m.AvatarUrl
    };

    private static SpecialistStatsResponse ToStatsResponse(this Specialist m) => new()
    {
        ExperienceYears = m.ExperienceYears,
        Rating = m.Rating,
        ReviewsCount = m.ReviewsCount,
        CompletedOrdersCount = m.CompletedOrdersCount,
        RepeatOrdersCount = m.RepeatOrdersCount
    };

    private static SpecialistCalendarResponse ToCalendarResponse(this Specialist m) => new()
    {
        Timezone = "Europe/Moscow",
        DayOverrides = new List<SpecialistCalendarDayOverrideResponse>(),
        BookedSlots = m.BookedSlots.Select(b => b.ToBookedSlotResponse()).ToList(),
        AvailabilityWindows = m.Availabilities.Select(a => a.ToAvailabilityWindowResponse()).ToList(),
        BookingSettings = new SpecialistCalendarBookingSettingsResponse
        {
            DayStartTime = "09:00",
            DayEndTime = "18:00",
            SlotStepMinutes = 30,
            DefaultDurationMinutes = 60
        },
        AvailabilityRules = new List<object>(),
        AvailabilityOverrides = new List<object>()
    };

    private static SpecialistDetailsResponse ToDetailsResponse(this Specialist m)
    {
        var d = m.Details ?? new Details();

        return new SpecialistDetailsResponse
        {
            ExperienceLabel = d.ExperienceLabel ?? "",
            ExperienceDurationValue = d.ExperienceDurationValue,
            ExperienceDurationUnit = d.ExperienceDurationUnit.HasValue
                ? SpecialistEnumMapper.MapExperienceUnit(d.ExperienceDurationUnit.Value)
                : null,

            HousingType = SpecialistEnumMapper.MapHousingType(d.HousingType),
            HasChildrenUnderTen = SpecialistEnumMapper.MapChildrenPresence(d.HasChildrenUnderTen),

            PetTypes = m.PetTypes.Select(SpecialistEnumMapper.MapPetType).ToList(),
            PetSizes = m.PetSizes.Select(SpecialistEnumMapper.MapPetSize).ToList(),
            PetAges = m.PetAges.Select(SpecialistEnumMapper.MapPetAge).ToList(),

            Advantages = m.Advantages.Select(a => a.Title).ToList(),
            About = d.About ?? ""
        };
    }

    private static SpecialistServiceResponse ToServiceResponse(this ServiceOffer s) => new()
    {
        Id = s.Id!.Value,
        Name = s.Name ?? "",
        Price = s.Price,
        PriceUnit = SpecialistEnumMapper.MapPriceUnit(s.PriceUnit),
        LocationLabel = s.LocationLabel ?? "",
        ServiceId = SpecialistEnumMapper.MapServiceType(s.Type),
        BookingPolicy = s.Type.ToBookingPolicyResponse()
    };

    private static SpecialistServiceBookingPolicyResponse ToBookingPolicyResponse(this ServiceType type)
    {
        return type switch
        {
            ServiceType.Walking => new SpecialistServiceBookingPolicyResponse
            {
                Mode = "fixed_slot",
                Duration = new()
                {
                    DefaultDurationMinutes = 60,
                    MinDurationMinutes = 30,
                    MaxDurationMinutes = 180,
                    DurationStepMinutes = 30
                },
                Buffer = new()
                {
                    HasBufferBefore = true,
                    BufferBeforeMinutes = 15,
                    HasBufferAfter = true,
                    BufferAfterMinutes = 15
                },
                AllowsClientComment = true,
                RequiresSpecialistConfirmation = false
            },

            ServiceType.Boarding => new SpecialistServiceBookingPolicyResponse
            {
                Mode = "multi_day_stay",
                MultiDay = new()
                {
                    AllowsMultiDayBooking = true,
                    MinStayDays = 1,
                    MaxStayDays = 14,
                    CheckInTime = "12:00",
                    CheckOutTime = "11:00"
                },
                AllowsClientComment = true,
                RequiresSpecialistConfirmation = true
            },

            _ => new SpecialistServiceBookingPolicyResponse
            {
                Mode = "fixed_slot",
                Duration = new() { DefaultDurationMinutes = 60 },
                AllowsClientComment = true,
                RequiresSpecialistConfirmation = false
            }
        };
    }

    private static ReviewResponse ToReviewResponse(this Review r) => new()
    {
        AuthorName = r.AuthorName,
        Rating = r.Rating,
        Text = r.Text ?? "",
        ServiceTitle = r.ServiceTitle,
        PetName = r.PetName,
        CreatedAt = r.CreatedAt,
        ReplyText = r.ReplyText,
        ReplyCreatedAt = r.ReplyCreatedAt
    };

    private static GalleryResponse ToGalleryResponse(this Gallery g) => new()
    {
        ImageUrl = g.ImageUrl,
        Alt = g.Alt ?? ""
    };

    // ==================== ToShortResponse для списка специалистов ====================
    public static SpecialistShortResponse ToShortResponse(this Specialist model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new SpecialistShortResponse
        {
            Id = model.Id,
            Slug = model.Slug,
            Name = $"{model.FirstName} {model.LastName}".Trim(),
            AvatarUrl = model.AvatarUrl,

            City = model.City,
            District = model.District ?? "",

            Description = model.Description ?? model.Details?.About ?? "",

            ExperienceYears = model.ExperienceYears,
            Rating = model.Rating,
            ReviewsCount = model.ReviewsCount,

            Location = new GeoPointResponse
            {
                Lat = model.Latitude ?? 0,
                Lon = model.Longitude ?? 0
            },

            Services = model.Services?.Select(s => new ServiceShortResponse
            {
                ServiceId = SpecialistEnumMapper.MapServiceType(s.Type),
                PetTypes = model.PetTypes?.Select(SpecialistEnumMapper.MapPetType).ToList()
                           ?? new List<string>(),
                PriceFrom = s.Price
            }).ToList() ?? new List<ServiceShortResponse>()
        };
    }

    // ==================== Вспомогательные методы для Calendar ====================
    private static SpecialistCalendarBookedSlotResponse ToBookedSlotResponse(this BookedSlot b) => new()
    {
        Id = b.Id.ToString(),
        Date = b.Date.ToString("yyyy-MM-dd"),
        StartTime = b.StartTime.ToString("HH:mm"),
        EndTime = b.EndTime.ToString("HH:mm"),
        ServiceIds = b.ServiceIds.Select(id => id.ToString()).ToList()
    };

    private static SpecialistCalendarAvailabilityWindowResponse ToAvailabilityWindowResponse(this Availability a) => new()
    {
        Id = a.Id.ToString(),
        Date = a.Date.ToString("yyyy-MM-dd"),
        StartTime = a.StartTime.ToString("HH:mm"),
        EndTime = a.EndTime.ToString("HH:mm"),
        ServiceIds = a.ServiceIds.Select(id => id.ToString()).ToList()
    };
}