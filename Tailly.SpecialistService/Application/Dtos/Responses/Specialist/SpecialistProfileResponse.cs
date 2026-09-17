using Tailly.SpecialistService.Application.Dtos.Responses.Calendar;
using Tailly.SpecialistService.Application.Dtos.Responses.Gallery;
using Tailly.SpecialistService.Application.Dtos.Responses.Reviews;
using Tailly.SpecialistService.Application.Dtos.Responses.Services;

namespace Tailly.SpecialistService.Application.Dtos.Responses.Specialist;

public sealed class SpecialistProfileResponse
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public SpecialistMainResponse Main { get; set; } = new();
    public SpecialistStatsResponse Stats { get; set; } = new();
    public CalendarResponse Calendar { get; set; } = new();
    public SpecialistDetailsResponse Details { get; set; } = new();
    public List<ServiceOfferResponse> Services { get; set; } = [];
    public List<ReviewResponse> Reviews { get; set; } = [];
    public List<GalleryItemResponse> SpecialistGallery { get; set; } = [];
}