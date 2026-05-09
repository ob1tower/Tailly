using Tailly.SpecialistService.Application.Dtos.Requests.Specialist;
using Tailly.SpecialistService.Application.Dtos.Responses.Calendar;
using Tailly.SpecialistService.Application.Dtos.Responses.Gallery;
using Tailly.SpecialistService.Application.Dtos.Responses.Reviews;
using Tailly.SpecialistService.Application.Dtos.Responses.Services;
using Tailly.SpecialistService.Application.Dtos.Responses.Specialist;
using Tailly.SpecialistService.Core.Models.Gallery;
using Tailly.SpecialistService.Core.Models.Reviews;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Application.Mappers;

public static class SpecialistResponseMapper
{
    public static SpecialistProfileResponse ToResponse(Specialist specialist)
    {
        return new SpecialistProfileResponse
        {
            Id = specialist.Id,
            Slug = specialist.Slug,

            Main = new SpecialistMainResponse
            {
                FirstName = specialist.FirstName ?? "",
                LastName = specialist.LastName ?? "",
                MiddleName = specialist.MiddleName ?? "",
                City = specialist.City ?? "",
                District = specialist.District ?? "",
                Phone = specialist.Phone ?? "",
                Email = specialist.Email ?? "",
                AvatarUrl = specialist.AvatarUrl ?? ""
            },

            Calendar = specialist.Calendar != null
                ? new CalendarResponse
                {
                    AvailabilityWindows = specialist.Calendar.AvailabilityWindows?
                        .Select(x => new AvailabilityWindowResponse
                        {
                            Date = x.Date.ToString("yyyy-MM-dd"),
                            StartTime = x.StartTime.ToString("HH:mm"),
                            EndTime = x.EndTime.ToString("HH:mm")
                        })
                        .ToList() ?? [],

                    DayOverrides = specialist.Calendar.DayOverrides?
                        .Select(x => new ManualOverrideResponse
                        {
                            Date = x.Date.ToString("yyyy-MM-dd"),
                            Status = x.Status.ToString().ToLower()
                        })
                        .ToList() ?? [],

                    BookedSlots = specialist.Calendar.BookedSlots?
                        .Select(x => new BookedSlotResponse
                        {
                            Date = x.Date.ToString("yyyy-MM-dd"),
                            StartTime = x.StartTime.ToString("HH:mm"),
                            EndTime = x.EndTime.ToString("HH:mm")
                        })
                        .ToList() ?? []
                }
                : new CalendarResponse
                {
                    AvailabilityWindows = [],
                    DayOverrides = [],
                    BookedSlots = []
                },

            Stats = new SpecialistStatsResponse
            {
                ExperienceYears = specialist.ExperienceYears,
                Rating = specialist.Rating,
                ReviewsCount = specialist.ReviewsCount,
                CompletedOrdersCount = specialist.CompletedOrdersCount,
                RepeatOrdersCount = specialist.RepeatOrdersCount
            },

            Details = specialist.Details != null
                ? ToDetailsResponse(specialist.Details)
                : new SpecialistDetailsResponse
                {
                    HousingType = "",
                    HasChildrenUnderTen = "",
                    About = "",
                    PetSizes = [],
                    PetAges = [],
                    PetTypes = []
                },

            Services = specialist.Services?
                .Select(ToServiceOfferResponse)
                .ToList() ?? [],

            Reviews = specialist.Reviews?
                .Select(ToReviewResponse)
                .ToList() ?? [],

            SpecialistGallery = specialist.SpecialistGallery?
                .Select(ToGalleryItemResponse)
                .ToList() ?? []
        };
    }

    public static SpecialistListItemResponse ToShortResponse(Specialist specialist)
    {
        var services = specialist.Services?
            .GroupBy(s => s.Name)
            .Select(g => new ServiceShortResponse
            {
                ServiceId = SpecialistEnumMapper.MapServiceType(g.Key),
                PetTypes = [],
                PriceFrom = g.Min(x => x.Price),
                PriceTo = g.Count() > 1 ? g.Max(x => x.Price) : null,
                DurationMinutes = null,
                Note = null
            })
            .ToList() ?? [];

        return new SpecialistListItemResponse
        {
            Id = specialist.Id,
            Name = $"{specialist.FirstName} {specialist.LastName}".Trim(),
            AvatarUrl = specialist.AvatarUrl,
            City = specialist.City ?? "",
            District = specialist.District ?? "",
            Description = specialist.Details?.About,
            Rating = specialist.Rating,
            ReviewsCount = specialist.ReviewsCount,
            ExperienceYears = specialist.ExperienceYears,

            Location = (specialist.Latitude.HasValue && specialist.Longitude.HasValue)
                ? new LocationResponse
                {
                    Lat = specialist.Latitude.Value,
                    Lon = specialist.Longitude.Value
                }
                : new LocationResponse(),

            Services = services
        };
    }

    private static SpecialistDetailsResponse ToDetailsResponse(Details details)
    {
        return new SpecialistDetailsResponse
        {
            HousingType =
                SpecialistEnumMapper.MapHousingType(
                    details.HousingType
                ),

            HasChildrenUnderTen =
                SpecialistEnumMapper.MapChildrenPresence(
                    details.HasChildrenUnderTen
                ),

            About = details.About ?? "",

            PetSizes = details.PetSizes?
                .Select(SpecialistEnumMapper.MapPetSize)
                .ToList() ?? [],

            PetAges = details.PetAges?
                .Select(SpecialistEnumMapper.MapPetAge)
                .ToList() ?? [],

            PetTypes = details.PetTypes?
                .Select(SpecialistEnumMapper.MapPetType)
                .ToList() ?? []
        };
    }

    private static ServiceOfferResponse ToServiceOfferResponse(ServiceOffer service)
    {
        return new ServiceOfferResponse
        {
            Id = service.Id,
            Name = SpecialistEnumMapper.MapServiceType(service.Name),
            Description = service.Description ?? "",
            Price = service.Price,

            PriceUnit =
                SpecialistEnumMapper.MapPriceUnit(
                    service.PriceUnit
                )
        };
    }

    private static ReviewResponse ToReviewResponse(Review review)
    {
        return new ReviewResponse
        {
            Id = review.Id,
            OrderId = review.OrderId,
            AuthorName = review.AuthorName ?? "",
            Text = review.Text ?? "",
            Rating = review.Rating,
            ServiceTitle = review.ServiceTitle ?? "",
            PetName = review.PetName ?? "",
            CreatedAt = review.CreatedAt,

            Photos = review.Photos?.ToList() ?? [],

            SpecialistReply = !string.IsNullOrWhiteSpace(review.ReplyText)
            ? new ReviewReplyResponse
            {
                Text = review.ReplyText,
                CreatedAt = review.ReplyCreatedAt ?? DateTime.UtcNow
            }
            : null
        };
    }

    private static GalleryItemResponse ToGalleryItemResponse(GalleryItem gallery)
    {
        return new GalleryItemResponse
        {
            Id = gallery.Id,
            ImageUrl = gallery.ImageUrl ?? "",
            Alt = gallery.Alt ?? "",
            Order = gallery.Order
        };
    }

    public static Details ToDetails(UpdateSpecialistDetailsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return new Details
        {
            HousingType = SpecialistEnumMapper.ParseHousingType(request.HousingType),
            HasChildrenUnderTen = SpecialistEnumMapper.ParseChildrenPresence(request.HasChildrenUnderTen),
            About = request.About,
            PetSizes = request.PetSizes?
                .Select(SpecialistEnumMapper.ParsePetSize)
                .ToList() ?? [],
            PetAges = request.PetAges?
                .Select(SpecialistEnumMapper.ParsePetAge)
                .ToList() ?? [],
            PetTypes = request.PetTypes?
                .Select(SpecialistEnumMapper.ParsePetType)
                .ToList() ?? [],
            SpecialistGallery = request.SpecialistGallery?
                .Select(x => new GalleryItem
                {
                    ImageUrl = x.ImageUrl,
                    Alt = x.Alt ?? ""
                })
                .ToList() ?? []
        };
    }
}