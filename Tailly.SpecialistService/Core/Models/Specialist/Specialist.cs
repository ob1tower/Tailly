using Tailly.SpecialistService.Core.Models.Calendars;
using Tailly.SpecialistService.Core.Models.Gallery;
using Tailly.SpecialistService.Core.Models.Reviews;

namespace Tailly.SpecialistService.Core.Models.Specialist;

public class Specialist
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public int CompletedOrdersCount { get; set; }
    public int RepeatOrdersCount { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Details Details { get; set; } = null!;
    public List<ServiceOffer> Services { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<GalleryItem> SpecialistGallery { get; set; } = [];
    public Calendar Calendar { get; set; } = null!;
}