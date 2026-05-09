namespace Tailly.ClientProfileService.Application.Clients;

public sealed class PetInternalDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = default!;
}