using Tailly.SpecialistService.Core.Entities.Calendar;
using Tailly.SpecialistService.Core.Entities.Details;
using Tailly.SpecialistService.Core.Entities.Gallery;
using Tailly.SpecialistService.Core.Entities.Reviews;
using Tailly.SpecialistService.Core.Entities.Services;
using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Core.Models.Calendar;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Infrastructure.Mappers;

public static class SpecialistEntityMapper
{
    public static SpecialistEntity ToEntity(Specialist model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

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
            Description = model.Description,
            ExperienceYears = model.ExperienceYears,
            Rating = model.Rating,
            ReviewsCount = model.ReviewsCount,
            CompletedOrdersCount = model.CompletedOrdersCount,
            RepeatOrdersCount = model.RepeatOrdersCount,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            CreatedAt = model.CreatedAt
        };
    }

    public static Specialist? ToDomain(this SpecialistEntity? entity)
    {
        if (entity == null)
            return null;

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

            Description = entity.Description,
            ExperienceYears = entity.ExperienceYears,

            Rating = entity.Rating,
            ReviewsCount = entity.ReviewsCount,
            CompletedOrdersCount = entity.CompletedOrdersCount,
            RepeatOrdersCount = entity.RepeatOrdersCount,

            Latitude = entity.Latitude,
            Longitude = entity.Longitude,

            Details = entity.Details?.ToDomain(),

            Services = entity.Services.Select(s => s.ToDomain()).ToList(),
            Reviews = entity.Reviews.Select(r => r.ToDomain()).ToList(),
            Gallery = entity.Gallery.Select(g => g.ToDomain()).ToList(),

            Availabilities = entity.Availabilities.Select(a => a.ToDomain()).ToList(),
            BookedSlots = entity.BookedSlots.Select(b => b.ToDomain()).ToList(),

            AvailabilityWeekdays = entity.AvailabilityWeekdays
                .Select(w => new AvailabilityWeekday { Weekday = w.Weekday })
                .ToList(),

            PetTypes = entity.PetTypes.Select(p => p.PetType).ToList(),
            PetSizes = entity.PetSizes.Select(p => p.PetSize).ToList(),
            PetAges = entity.PetAges.Select(p => p.PetAge).ToList(),
            Advantages = entity.Advantages.Select(a => new Advantage { Title = a.Title }).ToList()
        };
    }

    public static Specialist ToDomainRequired(this SpecialistEntity entity)
    {
        return ToDomain(entity) ?? throw new InvalidOperationException("Entity cannot be null");
    }

    private static Details? ToDomain(this DetailsEntity? entity)
    {
        if (entity == null) return null;

        return new Details
        {
            HousingType = entity.HousingType,
            HasChildrenUnderTen = entity.HasChildrenUnderTen,
            About = entity.About ?? "",
            ExperienceLabel = entity.ExperienceLabel ?? "",
            ExperienceDurationValue = entity.ExperienceDurationValue,
            ExperienceDurationUnit = entity.ExperienceDurationUnit
        };
    }

    public static ServiceOffer ToDomain(this ServiceEntity entity)
    {
        return new ServiceOffer
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
            PriceUnit = entity.PriceUnit,
            LocationLabel = entity.LocationLabel,
            Type = entity.Type
        };
    }

    public static Review ToDomain(this ReviewEntity entity)
    {
        return new Review
        {
            Id = entity.Id,
            AuthorName = entity.AuthorName,
            Rating = entity.Rating,
            Text = entity.Text,
            ServiceTitle = entity.ServiceTitle,
            PetName = entity.PetName,
            CreatedAt = entity.CreatedAt,
            ReplyText = entity.ReplyText,
            ReplyCreatedAt = entity.ReplyCreatedAt
        };
    }

    public static Gallery ToDomain(this GalleryEntity entity)
    {
        return new Gallery
        {
            Id = entity.Id,
            ImageUrl = entity.ImageUrl,
            Alt = entity.Alt
        };
    }

    public static Availability ToDomain(this AvailabilityEntity entity)
    {
        return new Availability
        {
            Id = entity.Id,
            Date = entity.Date,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            ServiceIds = entity.Services.Select(x => x.ServiceId).ToList()
        };
    }

    public static BookedSlot ToDomain(this BookedSlotEntity entity)
    {
        return new BookedSlot
        {
            Id = entity.Id,
            Date = entity.Date,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            OrderId = entity.OrderId,
            ServiceIds = entity.Services.Select(x => x.ServiceId).ToList()
        };
    }

    public static void UpdateEntity(SpecialistEntity entity, Specialist model)
    {
        entity.FirstName = model.FirstName;
        entity.LastName = model.LastName;
        entity.City = model.City;
        entity.District = model.District;
        entity.Phone = model.Phone;

        // DETAILS
        if (model.Details != null)
        {
            entity.Details ??= new DetailsEntity();

            entity.Details.About = model.Details.About;
            entity.Details.ExperienceLabel = model.Details.ExperienceLabel;
        }

        // SERVICES
        entity.Services ??= new List<ServiceEntity>();
        var incoming = model.Services ?? new List<ServiceOffer>();

        // ❌ УДАЛЕНИЕ
        var toRemove = entity.Services
            .Where(e => incoming.All(i => i.Id != e.Id))
            .ToList();

        foreach (var r in toRemove)
            entity.Services.Remove(r);

        foreach (var s in incoming)
        {
            // 🔥 ЕСЛИ НЕТ ID → ЭТО CREATE
            if (s.Id == null || s.Id == Guid.Empty)
            {
                entity.Services.Add(new ServiceEntity
                {
                    Id = Guid.NewGuid(),
                    SpecialistId = entity.Id,
                    Name = s.Name,
                    Price = s.Price,
                    PriceUnit = s.PriceUnit,
                    LocationLabel = s.LocationLabel,
                    Type = s.Type,
                    CreatedAt = DateTime.UtcNow
                });

                continue;
            }

            // 🔥 ЕСЛИ ЕСТЬ ID → UPDATE
            var existing = entity.Services.FirstOrDefault(e => e.Id == s.Id);

            if (existing != null)
            {
                existing.Name = s.Name;
                existing.Price = s.Price;
                existing.PriceUnit = s.PriceUnit;
                existing.LocationLabel = s.LocationLabel;
                existing.Type = s.Type;
            }
            else
            {
                // ⚠️ если id пришёл, но нет в БД → можно игнор или создать
                entity.Services.Add(new ServiceEntity
                {
                    Id = s.Id.Value,
                    SpecialistId = entity.Id,
                    Name = s.Name,
                    Price = s.Price,
                    PriceUnit = s.PriceUnit,
                    LocationLabel = s.LocationLabel,
                    Type = s.Type,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}