namespace Tailly.SpecialistService.Application.Dtos.Responses.Services;

public sealed class ServiceOfferResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = default!;
}