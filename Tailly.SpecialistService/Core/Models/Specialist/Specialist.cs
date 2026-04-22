using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Calendar;

namespace Tailly.SpecialistService.Core.Models.Specialist;

public class Specialist
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
    public string About { get; set; } = string.Empty;
    public Details? Details { get; set; }
    public List<ServiceOffer> Services { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<Gallery> Gallery { get; set; } = [];
    public List<Availability> Availabilities { get; set; } = [];
    public List<BookedSlot> BookedSlots { get; set; } = [];
    public List<AvailabilityWeekday> AvailabilityWeekdays { get; set; } = [];
    public List<PetType> PetTypes { get; set; } = [];
    public List<PetSize> PetSizes { get; set; } = [];
    public List<PetAge> PetAges { get; set; } = [];
    public List<Advantage> Advantages { get; set; } = [];
}