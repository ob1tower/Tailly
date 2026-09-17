using Tailly.ClientProfileService.Core.Enums;

namespace Tailly.ClientProfileService.Core.Models;

public class Breed
{
    public Guid Id { get; set; }
    public PetType? Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}