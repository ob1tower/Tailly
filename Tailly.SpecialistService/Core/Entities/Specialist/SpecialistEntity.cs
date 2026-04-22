using Tailly.SpecialistService.Core.Entities.Calendar;
using Tailly.SpecialistService.Core.Entities.Details;
using Tailly.SpecialistService.Core.Entities.Gallery;
using Tailly.SpecialistService.Core.Entities.Reviews;
using Tailly.SpecialistService.Core.Entities.Services;

namespace Tailly.SpecialistService.Core.Entities.Specialist;

public class SpecialistEntity
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public int CompletedOrdersCount { get; set; }
    public int RepeatOrdersCount { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<AvailabilityWeekdayEntity> AvailabilityWeekdays { get; set; } = [];
    public ICollection<ServiceEntity> Services { get; set; } = [];
    public ICollection<AvailabilityEntity> Availabilities { get; set; } = [];
    public ICollection<BookedSlotEntity> BookedSlots { get; set; } = [];
    public ICollection<ReviewEntity> Reviews { get; set; } = [];
    public ICollection<GalleryEntity> Gallery { get; set; } = [];
    public ICollection<AdvantageEntity> Advantages { get; set; } = [];
    public ICollection<PetTypeEntity> PetTypes { get; set; } = [];
    public ICollection<PetSizeEntity> PetSizes { get; set; } = [];
    public ICollection<PetAgeEntity> PetAges { get; set; } = [];
    public DetailsEntity? Details { get; set; }
}