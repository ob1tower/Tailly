using Tailly.ClientProfileService.Core.Enums;

namespace Tailly.ClientProfileService.Core.Entities;

public class BreedEntity
{
    public Guid Id { get; set; }
    public PetType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}