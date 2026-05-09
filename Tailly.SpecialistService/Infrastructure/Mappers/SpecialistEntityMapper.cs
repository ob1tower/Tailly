using Tailly.SpecialistService.Application.Dtos.Responses.Home;
using Tailly.SpecialistService.Core.Entities.Calendar;
using Tailly.SpecialistService.Core.Entities.Details;
using Tailly.SpecialistService.Core.Entities.Gallery;
using Tailly.SpecialistService.Core.Entities.Reviews;
using Tailly.SpecialistService.Core.Entities.Services;
using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Core.Models.Calendars;
using Tailly.SpecialistService.Core.Models.Gallery;
using Tailly.SpecialistService.Core.Models.Reviews;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Infrastructure.Mappers;

public static class SpecialistEntityMapper
{
    public static Specialist ToModel(SpecialistEntity entity)
    {
        return new Specialist
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Slug = entity.Slug,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            MiddleName = entity.MiddleName,
            City = entity.City,
            District = entity.District,
            Phone = entity.Phone,
            Email = entity.Email,
            AvatarUrl = entity.AvatarUrl,
            ExperienceYears = entity.ExperienceYears,
            Rating = entity.Rating,
            ReviewsCount = entity.ReviewsCount,
            CompletedOrdersCount = entity.CompletedOrdersCount,
            RepeatOrdersCount = entity.RepeatOrdersCount,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            CreatedAt = entity.CreatedAt
        };
    }

    public static Specialist ToFullModel(SpecialistEntity entity)
    {
        var model = ToModel(entity);

        if (entity.Details != null)
        {
            model.Details = new Details
            {
                HousingType = entity.Details.HousingType,
                HasChildrenUnderTen = entity.Details.HasChildrenUnderTen,
                About = entity.Details.About,

                PetSizes = entity.Details.PetSizes.Select(p => p.PetSize).ToList(),
                PetAges = entity.Details.PetAges.Select(p => p.PetAge).ToList(),
                PetTypes = entity.Details.PetTypes.Select(p => p.PetType).ToList()
            };
        }

        model.Services = entity.Services?.Select(ToServiceModel).ToList() ?? new();
        model.Reviews = entity.Reviews?.Select(ToReviewModel).ToList() ?? new();

        model.SpecialistGallery = entity.SpecialistGallery
            .OrderBy(g => g.Order)
            .Select(ToGalleryItem).ToList();

        if (entity.Calendar != null)
        {
            model.Calendar = ToCalendarModel(entity.Calendar);
        }

        return model;
    }

    public static ServiceOffer ToServiceModel(ServiceEntity entity)
    {
        return new ServiceOffer
        {
            Id = entity.Id,
            SpecialistId = entity.SpecialistId,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            PriceUnit = entity.PriceUnit
        };
    }

    public static ServiceEntity ToServiceEntity(Guid specialistId, ServiceOffer model)
    {
        return new ServiceEntity
        {
            Id = Guid.NewGuid(),
            SpecialistId = specialistId,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            PriceUnit = model.PriceUnit
        };
    }

    public static void UpdateServiceEntity(ServiceEntity entity, ServiceOffer model)
    {
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Price = model.Price;
        entity.PriceUnit = model.PriceUnit;
    }

    public static Review ToReviewModel(ReviewEntity entity)
    {
        return new Review
        {
            Id = entity.Id,
            SpecialistId = entity.SpecialistId,
            OrderId = entity.OrderId,
            AuthorName = entity.AuthorName,
            ServiceTitle = entity.ServiceTitle,
            PetName = entity.PetName,
            Rating = entity.Rating,
            Text = entity.Text,
            CreatedAt = entity.CreatedAt,
            ReplyText = entity.ReplyText,
            ReplyCreatedAt = entity.ReplyCreatedAt,
            Photos = entity.Photos ?? []
        };
    }

    public static GalleryItem ToGalleryItem(SpecialistGalleryEntity entity) => new()
    {
        Id = entity.Id,
        Order = entity.Order,
        ImageUrl = entity.ImageUrl,
        Alt = entity.Alt
    };

    public static void MapMainInfoToEntity(SpecialistEntity entity,
        string firstName, string lastName, string? middleName,
        string city, string district, string phone, string? avatarUrl)
    {
        entity.FirstName = firstName;
        entity.LastName = lastName;
        entity.MiddleName = middleName;
        entity.City = city;
        entity.District = district;
        entity.Phone = phone;
        entity.AvatarUrl = avatarUrl;
    }

    public static void MapDetailsToEntity(DetailsEntity entity, Details details)
    {
        entity.HousingType = details.HousingType;
        entity.HasChildrenUnderTen = details.HasChildrenUnderTen;
        entity.About = details.About;
    }

    public static SpecialistEntity ToEntity(Specialist model)
    {
        return new SpecialistEntity
        {
            Id = model.Id,
            UserId = model.UserId,
            Slug = model.Slug,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            City = model.City,
            District = model.District,
            Phone = model.Phone,
            Email = model.Email,
            AvatarUrl = model.AvatarUrl,
            ExperienceYears = model.ExperienceYears,
            Rating = model.Rating,
            ReviewsCount = model.ReviewsCount,
            CompletedOrdersCount = model.CompletedOrdersCount,
            RepeatOrdersCount = model.RepeatOrdersCount,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            CreatedAt = model.CreatedAt,

            Details = model.Details == null
            ? null
            : new DetailsEntity
            {
                Id = Guid.NewGuid(),

                About = model.Details.About,
                HousingType = model.Details.HousingType,
                HasChildrenUnderTen = model.Details.HasChildrenUnderTen,

                PetSizes = model.Details.PetSizes
                    .Select(x => new PetSizeEntity
                    {
                        Id = Guid.NewGuid(),
                        PetSize = x
                    })
                    .ToList(),

                PetAges = model.Details.PetAges
                    .Select(x => new PetAgeEntity
                    {
                        Id = Guid.NewGuid(),
                        PetAge = x
                    })
                    .ToList(),

                PetTypes = model.Details.PetTypes
                    .Select(x => new PetTypeEntity
                    {
                        Id = Guid.NewGuid(),
                        PetType = x
                    })
                    .ToList()
            }
        };
    }

    public static Calendar ToCalendarModel(CalendarEntity entity)
    {
        return new Calendar
        {
            Id = entity.Id,
            SpecialistId = entity.SpecialistId,
            Timezone = entity.Timezone,

            DayOverrides = entity.DayOverrides?
                .Select(x => new CalendarDayOverride
                {
                    Id = x.Id,
                    Date = x.Date,
                    Status = x.Status
                })
                .ToList() ?? [],

            BookedSlots = entity.BookedSlots?
                .Select(x => new CalendarBookedSlot
                {
                    Id = x.Id,
                    Date = x.Date,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    OrderId = x.OrderId
                })
                .ToList() ?? [],

            AvailabilityWindows = entity.AvailabilityWindows?
                .Select(x => new CalendarAvailabilityWindow
                {
                    Id = x.Id,
                    Date = x.Date,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Comment = x.Comment
                })
                .ToList() ?? [],

            BookingSettings = entity.BookingSettings == null
                ? null
                : new CalendarBookingSettings
                {
                    DayStartTime = entity.BookingSettings.DayStartTime,
                    DayEndTime = entity.BookingSettings.DayEndTime,
                    SlotStepMinutes = entity.BookingSettings.SlotStepMinutes,
                    DefaultDurationMinutes =
                        entity.BookingSettings.DefaultDurationMinutes
                }
        };
    }

    public static HomeReviewResponse ToHomeReview(ReviewEntity entity)
    {
        return new HomeReviewResponse
        {
            Id = entity.Id,
            CreatedAtIso = entity.CreatedAt.ToString("O"),
            Rating = entity.Rating,
            Text = entity.Text,
            PetName = entity.PetName,
            OwnerName = entity.AuthorName,
            SitterId = entity.SpecialistId,
            SitterName = $"{entity.Specialist.FirstName} {entity.Specialist.LastName}",
            ServiceTitle = entity.ServiceTitle,
            PhotoUrls = entity.Photos
        };
    }
}