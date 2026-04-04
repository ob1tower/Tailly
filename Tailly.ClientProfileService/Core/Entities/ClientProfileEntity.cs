namespace Tailly.ClientProfileService.Core.Entities;

public class ClientProfileEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? CityId { get; set; }
    public string? AvatarUrl { get; set; }

    public ICollection<PetEntity> Pets { get; set; } = [];
}