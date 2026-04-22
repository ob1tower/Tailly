namespace Tailly.SpecialistService.Application.Dtos.Requests.SpecialistProfile;

public sealed class SpecialistServiceUpdateItem
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = default!;
    public string LocationLabel { get; set; } = default!;
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = "hour";
}