namespace Tailly.BookingService.Application.Dtos.Internal;

public sealed class SpecialistInternalDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string FullName { get; set; } = default!;
}