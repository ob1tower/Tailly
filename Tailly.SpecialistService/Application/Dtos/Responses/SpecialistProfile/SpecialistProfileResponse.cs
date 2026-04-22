using Tailly.SpecialistService.Application.Dtos.Responses.Calendar;
using Tailly.SpecialistService.Application.Dtos.Responses.Common;

namespace Tailly.SpecialistService.Application.Dtos.Responses.SpecialistProfile;

public sealed class SpecialistProfileResponse
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public SpecialistMainInfoResponse Main { get; set; } = default!;
    public SpecialistStatsResponse Stats { get; set; } = default!;
    public SpecialistCalendarResponse Calendar { get; set; } = new();
    public List<GalleryResponse> SpecialistGallery { get; set; } = [];
    public List<GalleryResponse> PetGallery { get; set; } = [];
    public SpecialistDetailsResponse Details { get; set; } = default!;
    public List<SpecialistServiceResponse> Services { get; set; } = [];
    public List<ReviewResponse> Reviews { get; set; } = [];
}