namespace Tailly.SpecialistService.Application.Dtos.Requests.Services;

public sealed class CreateServiceRequest
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = default!;
}