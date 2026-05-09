namespace Tailly.ClientProfileService.Application.Clients;

public sealed class ServiceInternalDto
{
    public Guid Id { get; set; }
    public Guid SpecialistId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = default!;
}