namespace Tailly.SpecialistService.Core.Models.Specialist;

public class Gallery
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}