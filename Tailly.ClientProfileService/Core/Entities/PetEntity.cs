using Tailly.ClientProfileService.Core.Enums;

namespace Tailly.ClientProfileService.Core.Entities;

public class PetEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public PetType? Type { get; set; }
    public int AgeYears { get; set; }
    public int AgeMonths { get; set; }
    public PetSize? Size { get; set; }
    public PetGender? Gender { get; set; }
    public PetAttitude? ToOtherPets { get; set; }
    public PetAttitude? ToKidsUnder10 { get; set; }
    public PetHomeAlone? StaysHomeAlone { get; set; }
    public PetVaccinated? Vaccinated { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid ClientId { get; set; }
    public ClientProfileEntity ClientProfile { get; set; } = default!;

    public Guid? BreedId { get; set; }
    public BreedEntity? Breed { get; set; } = default!;
}